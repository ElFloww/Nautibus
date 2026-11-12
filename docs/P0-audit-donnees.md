# P0 — Rapport de couverture des données Ametis

Audit du 2026-11-12, vers 11 h 50 (heure de Paris). Analyse faite sur les fichiers téléchargés ce jour et sur des instantanés des flux temps réel. Script d'analyse sans dépendance (décodage protobuf maison), hors dépôt.

## 1. Sources (P0.1)

| Source | URL | Format | Remarque |
|---|---|---|---|
| Jeu de données | https://transport.data.gouv.fr/datasets/ametis | — | Publié par Ametis, sous la responsabilité de CA Amiens Métropole. Exploitant : Keolis Amiens. |
| GTFS « complet » (horaires + TAD + scolaire + tracés) | https://www.data.gouv.fr/api/1/datasets/r/571e3014-066d-4bd6-9f73-fb8065c928c6 | GTFS zip, 2,2 Mo | Validité 2026-11-01 → 2027-05-01 ; mise à jour semestrielle. |
| GTFS « pour le GTFS-RT » | https://www.data.gouv.fr/api/1/datasets/r/5bc8b7dc-0d4e-48e7-b2ec-eccfe3702c19 | GTFS zip, 1,3 Mo | Validité **2 semaines** (2026-11-05 → 2026-11-18/19) ; republié régulièrement. |
| GTFS-RT Vehicle Positions | https://proxy.transport.data.gouv.fr/resource/ametis-amiens-gtfs-rt-vehicle-position | protobuf | Sans authentification. |
| GTFS-RT Trip Updates | https://proxy.transport.data.gouv.fr/resource/ametis-amiens-gtfs-rt-trip-update | protobuf | Sans authentification. |
| GTFS-RT Service Alerts | https://proxy.transport.data.gouv.fr/resource/ametis-amiens-gtfs-rt-alerts | protobuf | Sans authentification. **Vide** lors de l'audit. |
| GTFS-RT scolaire (Zenbus) | https://zenbus.net/gtfs/rt/poll.proto?src=true&dataset=amiensametisscolaire&type=JSON | **JSON** (pas protobuf) | Déclaré protobuf ; le validateur national échoue depuis 30 jours. |

- **API publique** : aucune autre API Ametis identifiée en dehors de ces flux.
- **Accès** : aucune clé, aucune limite de débit annoncée. Le proxy transport.data.gouv.fr est le point d'accès recommandé.
- **Licence** : `lov2` (Licence Ouverte / Etalab 2.0) selon l'API data.gouv.fr. Réutilisation commerciale ou non permise, avec **mention de la source** (Ametis / Amiens Métropole) et de la date de mise à jour. Une recherche web citait ODbL : non confirmé par l'API, qui fait foi ici.

## 2. Analyse GTFS (P0.2)

| Critère | GTFS complet | GTFS pour GTFS-RT |
|---|---|---|
| Lignes (`routes`) | 42, toutes bus (`route_type` 3), couleurs présentes | 22, bus, **aucune couleur** |
| Lignes | urbaines + M1/M2, R61-R68 (TAD), T33-T49 (scolaires) | urbaines + NAM1/NAM2/NAVC, pas de TAD ni scolaire |
| Arrêts | 1 028 arrêts + 424 zones d'arrêt (`location_type` 1) | 740 arrêts, pas de zones |
| Courses | 7 174 | 5 049 |
| `stop_times` | 239 531 ; colonnes GTFS-Flex (fenêtres de prise en charge, règles de réservation) | 172 822 ; `shape_dist_traveled` présent |
| Calendrier | 24 services `calendar` + 1 service seulement en `calendar_dates` ; 286 suppressions, 4 ajouts | 10 services ; 15 ajouts |
| Tracés (`shapes`) | 51 978 points, toutes les courses ont un `shape_id` | **absent** |
| `feed_info` | absent | absent |
| Heures > 24:00 | 933 (jusqu'à 24:37) | 622 |
| Fichiers non standard | `object_codes_extension.txt` (ignoré par le validateur) | — |
| Validateur national | 1 avertissement (temps de trajet nul), 234 arrêts en double (info) | 3 infos |

**Constat principal : les deux GTFS n'ont aucun identifiant commun** (0 `trip_id`, 0 `stop_id`, 0 `route_id`). Exemples : `AMI:7310120-GTFS_2026-PHIDIM-Dimanche-001` contre `2-10-6-5-074101`. Les lignes M1/M2 du GTFS complet s'appellent NAM1/NAM2 dans l'autre.

Rapprochement : les **740 arrêts** du GTFS pour GTFS-RT ont tous un arrêt du GTFS complet à moins de 15 m. Le rapprochement par coordonnées et nom est donc possible pour les arrêts ; pour les lignes, par `route_short_name` avec la table NAM1→M1, NAM2→M2 à confirmer.

**Services spéciaux** : TAD (R61-R68, GTFS-Flex, réservation) et lignes scolaires (T33-T49), uniquement dans le GTFS complet. Le TAD ne peut pas être présenté comme une ligne régulière (pas d'horaires fixes au sens strict).

## 3. Analyse temps réel (P0.3)

Instantanés de 11 h 52 à 11 h 56.

| Flux | Contenu observé | Qualité |
|---|---|---|
| Vehicle Positions | 79-80 véhicules ; tous ont `vehicle.id`, position, vitesse, cap ; 71/79 avec `trip_id`, **tous connus** du GTFS pour GTFS-RT | 6 `stop_id` inconnus (384, 957, 10000) ; validateur : E011 sur 30 jours sur 30 |
| Trip Updates | 85 courses, 2 916 `stop_time_update` ; retard (`delay`) et heure absolue présents ; `vehicle` renseigné partout | **E022** (heures successives non croissantes) systématique : 139 occurrences le jour de l'audit, 4 194 sur 30 jours |
| Service Alerts | 0 entité | Signalement public de juin 2026 : flux vide malgré des perturbations annoncées sur le site Ametis |
| Scolaire (Zenbus) | JSON, 13 entités, 6 avec course ; 2/6 retrouvées dans le GTFS complet avec le préfixe `SCO:` | Non conforme (JSON), validation impossible |

- **Fréquence** : en-tête du flux mis à jour toutes les **40 à 60 s** (11:53:38, 11:54:39, 11:55:20). Mesure sur 3 minutes seulement.
- **Fraîcheur** : positions en retard de 5 à 10 s sur l'en-tête ; au moins un véhicule avec une position vieille de **13 min** dans le flux. La gestion de l'obsolescence est indispensable.
- **Couverture à midi un jour de semaine** : positions sur 18 lignes sur 22, prévisions sur 19 lignes sur 22. Aucune mesure soir, week-end ou vacances : **à compléter** par une collecte continue (Worker P3.2).

## 4. Ce qu'on peut afficher et prédire

| Fonctionnalité | Lignes urbaines (22) | TAD R61-R68 | Scolaires T33-T49 | Statut |
|---|---|---|---|---|
| Liste lignes, arrêts, horaires théoriques | oui (GTFS complet) | adaptée (zones, réservation) | oui | Disponible |
| Couleurs de ligne | oui (GTFS complet) | oui | oui | Disponible |
| Tracés sur la carte | oui (GTFS complet) | oui | oui | Disponible |
| Positions véhicules | oui, si rapprochement GTFS-RT → GTFS complet | non | expérimental (JSON Zenbus) | **Adaptée** |
| Prochains passages temps réel (Trip Updates) | oui, après nettoyage E022 | non | non | **Adaptée** |
| Prédiction maison (P5) | oui : position, vitesse, `shape_dist_traveled` | non | non | Faisable |
| Perturbations (Service Alerts) | flux vide | — | — | **Bloquée** (aucune donnée observée) |
| Itinéraires OTP (P6) | GTFS complet + OSM | GTFS-Flex selon version OTP | oui | À vérifier en P6 |

## 5. Conséquences pour la suite (à valider)

1. **Deux imports GTFS** : le complet (référence réseau, tracés, couleurs) et le « GTFS-RT » (référence temps réel, rechargé chaque semaine). Une table de correspondance arrêts et lignes est calculée à chaque import, avec un niveau de confiance (distance, nom).
2. Le Worker temps réel doit : interroger toutes les 30 s, rejeter les positions trop anciennes (seuil à fixer, proposé : 2 min), journaliser `stop_id` inconnus et séquences non croissantes, ne jamais les corriger silencieusement.
3. Le décodage protobuf demandera un package (`Google.Protobuf` ou les liaisons GTFS-RT) : approbation requise.
4. Mention obligatoire de la source (Licence Ouverte 2.0) dans l'application.
5. À surveiller : expiration du GTFS pour GTFS-RT tous les 15 jours ; un import manqué rend le temps réel inutilisable.
