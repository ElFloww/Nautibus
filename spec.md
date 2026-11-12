# Nautibus (ex-Ametis Alternative) — Backlog de développement V2

## Vue d'ensemble

| Priorité | Lot                          | Objectif                                       |
| -------- | ---------------------------- | ---------------------------------------------- |
| P0       | Cadrage et audit des données | Vérifier ce qu'Ametis fournit réellement       |
| P1       | Socle technique              | Construire l'infrastructure et les fondations  |
| P2       | Import GTFS                  | Rendre le réseau consultable                   |
| P3       | Carte et temps réel          | Afficher les véhicules et les arrêts           |
| P4       | Comptes et personnalisation  | Ouvrir l'application aux utilisateurs          |
| P5       | Prédictions                  | Calculer des estimations personnalisées        |
| P6       | Routage                      | Intégrer OpenTripPlanner                       |
| P7       | PWA et notifications         | Ajouter les fonctionnalités avancées           |
| P8       | Historique et administration | Exploiter les données et superviser le système |
| P9       | Stabilisation et production  | Sécuriser et fiabiliser le service             |

---

## P0 — Audit des données Ametis

**Priorité : critique — à réaliser avant le développement fonctionnel.**

### P0.1 — Inventaire des sources

- [x] Identifier les portails open data Ametis et Amiens Métropole.
- [x] Identifier les éventuelles API publiques.
- [x] Identifier les fichiers GTFS statiques.
- [x] Identifier les flux GTFS Realtime.
- [x] Identifier les éventuelles conditions d'accès et limitations.
- [x] Identifier les conditions de réutilisation et licences.

### P0.2 — Analyse GTFS

- [x] Télécharger un jeu de données GTFS.
- [x] Analyser routes.txt.
- [x] Analyser stops.txt.
- [x] Analyser trips.txt.
- [x] Analyser stop_times.txt.
- [x] Analyser calendar.txt et calendar_dates.txt.
- [x] Analyser shapes.txt.
- [x] Identifier les services spéciaux.

### P0.3 — Analyse temps réel

- [x] Tester Vehicle Positions.
- [x] Tester Trip Updates.
- [x] Tester Service Alerts.
- [x] Vérifier les fréquences de mise à jour.
- [x] Vérifier les identifiants de véhicules et de courses.
- [ ] Mesurer la couverture réelle des services. *(partiel : un seul instantané en semaine à midi ; collecte continue à faire avec le Worker P3.2)*
- [x] Identifier les données manquantes ou incohérentes.

**Livrable :** rapport de couverture des données (`docs/P0-audit-donnees.md`), avec un tableau par service et par fonctionnalité disponible.

**Critère de sortie :** savoir précisément ce qu'il est possible d'afficher et de prédire pour chaque service Ametis.

---

## P1 — Socle technique et infrastructure

### P1.1 — Organisation du projet

- [ ] Créer le dépôt frontend Vue 3 / TypeScript. *(projet `frontend/` créé ; aucun dépôt Git)*
- [ ] Créer le dépôt backend .NET 10. *(aucun dépôt Git)*
- [x] Définir la structure Clean Architecture.
- [ ] Configurer les environnements Development, Preproduction et Production.
- [ ] Définir la stratégie de branches et de déploiement.
- [x] Ajouter les fichiers de configuration Docker. *(docker-compose.yml + deploy.sh ; pile complète démarrée et testée sous Docker Desktop le 2026-11-12 ; pas encore sur le LXC)*

### P1.2 — Infrastructure

- [ ] Préparer les conteneurs sur Proxmox.
- [ ] Déployer PostgreSQL.
- [ ] Activer PostGIS.
- [ ] Déployer Redis.
- [ ] Configurer les volumes persistants.
- [ ] Configurer les sauvegardes. *(« ./deploy.sh backup » manuel ; planification et restauration non faites)*
- [x] Configurer le reverse proxy. *(Nginx Proxy Manager du propriétaire → nginx du service web (port HTTP_PORT, 8080 par défaut) → API ; chaîne testée en local en simulant NPM)*
- [ ] Mettre en place HTTPS. *(assuré par Nginx Proxy Manager : proxy host, Let's Encrypt, websockets ; non exécuté)*
- [ ] Préparer les secrets et variables d'environnement.

### P1.3 — Fondations backend

- [x] Configurer ASP.NET Core.
- [x] Configurer Entity Framework Core.
- [x] Configurer les migrations. *(adapté : schéma versionné dans le projet DACPAC `AmetisAPI.Database`, pas de migrations EF)*
- [x] Configurer MediatR. *(adapté : Cortex.Mediator conservé, décision du propriétaire 2026-11-12)*
- [x] Configurer FluentValidation.
- [x] Configurer les logs structurés.
- [x] Ajouter un endpoint de santé.
- [x] Ajouter les premiers tests unitaires et d'intégration.

### P1.4 — Fondations frontend

- [x] Configurer Vue 3 et TypeScript.
- [x] Configurer Vuetify. *(adapté : Tailwind CSS à la place de Vuetify, décision du propriétaire 2026-11-12)*
- [x] Configurer Pinia.
- [x] Configurer Vue Router.
- [x] Créer le layout principal.
- [x] Créer les composants communs.
- [x] Mettre en place les thèmes clair et sombre.
- [x] Configurer le client API.

**Livrable :** application vide déployée sur l'infrastructure, accessible en HTTPS.

---

## P2 — Import et consultation du réseau

### P2.1 — Modèle de données

- [x] Créer les entités réseau.
- [x] Créer les entités lignes et arrêts.
- [x] Créer les entités courses et horaires.
- [x] Créer les entités calendriers.
- [x] Créer le stockage géographique des tracés.
- [x] Ajouter les index PostgreSQL et PostGIS nécessaires. *(adapté : index SQL Server, dont index spatial sur les arrêts)*

### P2.2 — Import GTFS

- [x] Développer le téléchargement GTFS.
- [x] Développer le parsing des fichiers.
- [x] Valider la structure des fichiers.
- [x] Normaliser les données.
- [x] Gérer les mises à jour sans doublons.
- [x] Gérer les suppressions et modifications.
- [x] Journaliser les imports.
- [x] Détecter les imports incomplets ou en échec.

### P2.3 — API réseau

- [x] Endpoint de recherche des lignes.
- [x] Endpoint de détail d'une ligne.
- [x] Endpoint de recherche des arrêts.
- [x] Endpoint de détail d'un arrêt.
- [x] Endpoint des horaires d'un arrêt.
- [x] Endpoint des prochains passages théoriques.
- [x] Endpoint des tracés de ligne.

### P2.4 — Interface réseau

- [x] Page d'accueil.
- [x] Liste des lignes.
- [x] Recherche de lignes et d'arrêts.
- [x] Page détaillée d'une ligne.
- [x] Page détaillée d'un arrêt.
- [x] Consultation des horaires.
- [x] Affichage des directions et parcours.

**Livrable :** consultation complète du réseau à partir des données statiques disponibles.

---

## P3 — Carte et données temps réel

### P3.1 — Carte

- [x] Intégrer MapLibre GL JS.
- [x] Configurer MapTiler. *(adapté : OpenFreeMap, gratuit et sans clé, décision du propriétaire 2026-11-12)*
- [x] Ajouter les fonds de carte clair et sombre. *(OpenFreeMap `liberty` et `dark`)*
- [x] Afficher les arrêts.
- [x] Afficher les tracés des lignes.
- [x] Ajouter la sélection d'un arrêt.
- [x] Ajouter la recherche géographique.

### P3.2 — Collecte GTFS-RT

- [x] Développer le Worker .NET.
- [x] Implémenter la collecte Vehicle Positions.
- [x] Implémenter la collecte Trip Updates.
- [x] Implémenter la collecte Service Alerts.
- [x] Gérer les erreurs et indisponibilités.
- [x] Vérifier les dates et la fraîcheur des données.
- [x] Normaliser les positions des véhicules.
- [x] Associer les véhicules aux courses.
- [x] Ajouter un niveau de confiance aux associations.

### P3.3 — Temps réel

- [ ] Stocker les dernières positions dans Redis. *(code écrit, non testé contre un Redis réel ; mémoire du processus en développement)*
- [x] Persister les positions historiques.
- [x] Développer les événements SignalR.
- [x] Diffuser les mises à jour des véhicules.
- [x] Afficher les véhicules sur la carte.
- [x] Afficher leur ligne et leur direction.
- [x] Afficher la date de dernière mise à jour.
- [x] Gérer les véhicules dont les données deviennent obsolètes.
- [x] Afficher les perturbations disponibles. *(adapté : page « Infos trafic » ; alertes officielles affichées dès qu'Ametis en publie — flux vide à ce jour — et déviations détectées par estimation, marquées comme telles)*

**Livrable :** carte interactive avec positions réelles des véhicules lorsque les données sont disponibles.

---

## P4 — Comptes et personnalisation

### P4.1 — Authentification

- [x] Intégrer ASP.NET Core Identity.
- [x] Développer l'inscription ouverte.
- [x] Développer la connexion.
- [x] Développer la déconnexion.
- [x] Développer la vérification de l'adresse e-mail. *(active dès que la section `Smtp` est renseignée ; vérifiée en dépôt de fichiers, pas contre un vrai serveur SMTP)*
- [x] Développer la récupération du mot de passe. *(même condition ; vérifiée en dépôt de fichiers)*
- [x] Configurer les cookies sécurisés.
- [x] Ajouter la protection CSRF. *(adapté : cookie SameSite=Strict + CORS restreint ; pas de jeton antiforgery)*
- [x] Ajouter le rate limiting.

### P4.2 — Utilisateurs

- [ ] Créer la gestion du profil.
- [ ] Créer les préférences utilisateur.
- [x] Gérer les favoris.
- [x] Gérer les arrêts favoris.
- [x] Gérer les lignes favorites.
- [ ] Gérer les préférences de carte.
- [ ] Gérer les préférences de notification.

### P4.3 — Interface

- [x] Pages d'inscription et de connexion.
- [x] Page de profil. *(minimal : e-mail, déconnexion, suppression du compte)*
- [x] Tableau de bord personnel. *(minimal : favoris sur la page d'accueil)*
- [x] Gestion des favoris.
- [ ] Gestion des préférences.

**Livrable :** application multi-utilisateurs avec données personnelles isolées.

---

## P5 — Moteur de prédiction

### P5.1 — Première version déterministe

> État 2026-11-12 : les prochains passages temps réel exposent l'estimation officielle des Trip Updates Ametis, avec provenance et niveau de confiance. Le modèle de prédiction propre n'est pas commencé.

- [ ] Définir le modèle de prédiction.
- [ ] Calculer la distance restante jusqu'à l'arrêt.
- [ ] Exploiter le tracé de la ligne.
- [ ] Exploiter la vitesse récente du véhicule.
- [ ] Prendre en compte les temps théoriques entre arrêts.
- [ ] Calculer une première ETA.
- [ ] Définir les cas où une prédiction est impossible.
- [ ] Ajouter la provenance de chaque estimation.
- [ ] Ajouter un niveau de confiance.

### P5.2 — Évaluation

- [x] Collecter les passages réellement observés. *(adapté : heures de passage publiées par Ametis dans les Trip Updates pour les arrêts desservis, table `rt.StopPassage`, 30 jours ; pas de mesure GPS indépendante)*
- [ ] Construire un jeu de données de validation.
- [ ] Calculer la MAE.
- [ ] Calculer la RMSE.
- [ ] Calculer l'erreur médiane.
- [ ] Mesurer les prédictions à ±1, ±2 et ±5 minutes.
- [ ] Comparer avec les horaires théoriques.
- [ ] Comparer avec les données officielles lorsqu'elles existent.

### P5.3 — Amélioration statistique

- [ ] Agréger les temps de parcours historiques.
- [ ] Segmenter les données par ligne et direction.
- [ ] Segmenter par période et jour.
- [ ] Identifier les ralentissements récurrents.
- [ ] Ajouter des facteurs correctifs statistiques.
- [ ] Évaluer les gains par rapport à la première version.
- [ ] Activer progressivement le modèle amélioré.

**Livrable :** prédictions personnalisées mesurées et comparées à des références.

---

## P6 — Recherche d'itinéraires avec OpenTripPlanner

### P6.1 — Infrastructure OTP

- [ ] Déployer OpenTripPlanner.
- [ ] Préparer les données GTFS compatibles.
- [ ] Préparer les données OpenStreetMap.
- [ ] Générer le graphe de transport.
- [ ] Vérifier la couverture géographique.
- [ ] Vérifier la prise en charge des services Ametis.

### P6.2 — Intégration backend

- [ ] Créer un client OpenTripPlanner.
- [ ] Isoler les appels OTP derrière une abstraction.
- [ ] Développer l'endpoint de recherche d'itinéraire.
- [ ] Normaliser les résultats.
- [ ] Gérer les erreurs et les délais d'attente.
- [ ] Intégrer les informations temps réel compatibles.

### P6.3 — Interface

- [ ] Formulaire de recherche de trajet.
- [ ] Recherche d'adresse et d'arrêt.
- [ ] Sélection du départ et de l'arrivée.
- [ ] Affichage des itinéraires alternatifs.
- [ ] Affichage des correspondances.
- [ ] Affichage des temps de marche et de transport.
- [ ] Affichage du trajet sur la carte.

**Livrable :** calcul d'itinéraires multimodaux opérationnel.

---

## P7 — PWA, notifications et hors ligne

### P7.1 — PWA

- [x] Configurer le manifest. *(manifeste minimal, icône SVG)*
- [x] Configurer le Service Worker. *(notifications push uniquement, pas de cache hors ligne)*
- [ ] Rendre l'application installable.
- [ ] Gérer les mises à jour de l'application.

### P7.2 — Hors ligne

- [ ] Mettre en cache le shell applicatif.
- [ ] Mettre en cache les lignes et arrêts.
- [ ] Mettre en cache les horaires statiques.
- [ ] Conserver les favoris localement.
- [ ] Afficher clairement l'état hors ligne.
- [ ] Gérer la synchronisation au retour du réseau.

### P7.3 — Notifications

- [x] Configurer Web Push et VAPID. *(sans package : chiffrement RFC 8291 et VAPID RFC 8292 avec les primitives .NET)*
- [x] Gérer les abonnements push.
- [x] Gérer la désinscription.
- [x] Développer les notifications de perturbation. *(alertes officielles et déviations détectées)*
- [x] Gérer les notifications liées aux favoris. *(choix « toutes les lignes » ou « mes lignes favorites »)*
- [ ] Ajouter les préférences de notification.
- [ ] Tester les navigateurs et plateformes compatibles. *(non fait : réception réelle d'une notification non vérifiée)*

**Livrable :** application installable avec fonctionnement hors ligne partiel et notifications.

---

## P8 — Historique et administration

### P8.1 — Historique

- [x] Stocker les positions GPS avec horodatage.
- [x] Mettre en place une politique de rétention de 30 jours.
- [ ] Développer les requêtes historiques.
- [ ] Développer l'API de rejeu.
- [ ] Afficher les déplacements sur la carte.
- [ ] Ajouter les contrôles de lecture et de vitesse.
- [ ] Conserver des agrégats statistiques si nécessaire.

### P8.2 — Administration

- [ ] Ajouter un rôle administrateur.
- [ ] Protéger la section d'administration.
- [ ] Créer le tableau de bord technique.
- [x] Afficher l'état des imports.
- [x] Afficher la fraîcheur des données.
- [ ] Afficher les erreurs de collecte.
- [ ] Afficher l'état des Workers.
- [ ] Afficher l'état des connexions temps réel.
- [ ] Gérer les utilisateurs.
- [ ] Journaliser les opérations sensibles.

**Livrable :** interface d'administration et historique des véhicules exploitables.

---

## P9 — Stabilisation et mise en production

- [ ] Ajouter les tests unitaires.
- [ ] Ajouter les tests d'intégration.
- [ ] Tester les imports sur des données réelles.
- [ ] Tester les accès simultanés.
- [ ] Tester les performances de la carte.
- [ ] Tester les déconnexions Redis et PostgreSQL.
- [ ] Tester les redémarrages des Workers.
- [ ] Configurer les sauvegardes automatiques.
- [ ] Tester une restauration complète.
- [ ] Configurer les alertes de supervision.
- [ ] Vérifier les règles de sécurité.
- [ ] Vérifier les mentions légales et la politique de confidentialité.
- [ ] Vérifier les obligations RGPD.
- [ ] Déployer en production.
- [ ] Mettre en place un suivi des erreurs et de la disponibilité.

**Livrable :** application publique stable, supervisée et restaurable.

---

## Ordre de développement recommandé

1. P0 — Audit des données.
2. P1 — Socle technique.
3. P2 — Import GTFS et consultation du réseau.
4. P3 — Carte et temps réel.
5. P4 — Comptes et personnalisation.
6. P5 — Prédictions.
7. P6 — Routage.
8. P7 — PWA et notifications.
9. P8 — Historique et administration.
10. P9 — Stabilisation et production.

Les lots P4 et P6 peuvent être partiellement développés en parallèle des travaux de données et de prédiction, à condition de ne pas bloquer les fondations.

## Définition du MVP

Le MVP doit être suffisamment utile pour être accessible publiquement, sans attendre toutes les fonctionnalités avancées.

Il comprend :

- Import des données statiques.
- Consultation des lignes et arrêts.
- Horaires théoriques.
- Carte interactive.
- Positions des véhicules lorsque disponibles.
- Première version des prochains passages.
- Inscription et connexion.
- Favoris.
- Déploiement HTTPS sur Proxmox.

Le moteur statistique avancé, le rejeu historique, les notifications, le mode hors ligne complet et l'administration approfondie peuvent arriver après cette première version.

## Règle de pilotage

Chaque fonctionnalité doit avoir :

- Un objectif fonctionnel clair.
- Des critères d'acceptation.
- Des tests adaptés.
- Une validation sur données réelles lorsque nécessaire.

Une fonctionnalité dépendant d'une source Ametis indisponible doit être identifiée comme bloquée ou adaptée, et non considérée comme terminée.
