# Journal des modifications

## 2026-11-12 — État des lieux avant développement

- Demande : développer toute l'application. Aucun code modifié : lecture de `spec.md` (aucune case cochée, P0 non commencé) et du template.
- Écart avec CLAUDE.md : le template est dans `AmetisAPI\AmetisAPI\` (solution `AmetisAPI.slnx`, projets `AmetisAPI.*`), pas `AmetisAPI\Ametis\` / `Ametis.slnx`.
- `dotnet build AmetisAPI.slnx` : 9 projets, 0 erreur, 22 avertissements (dont vulnérabilités connues `Microsoft.OpenApi` 2.4.1 et `System.Security.Cryptography.Pkcs` 9.0.4, transitives ; signalé, non corrigé).
- Tests : Core 11/11 réussis, API 1/1 réussi, Domain 0 test. Infrastructure **non lancé** (exige `DACPACPATH` et LocalDb).
- Arrêt : les points ouverts bloquant P1 (CLAUDE.md §9 : licences MediatR/AutoMapper, migration du template, authentification, géographie SQL Server) et l'audit P0 doivent être tranchés par le propriétaire avant tout développement.
- Décisions du propriétaire (réponses en chat) : garder Cortex.Mediator et Mapster ; Identity + cookies ; supprimer `AmetisAPI.APP` ; Claude mène l'audit P0. Reportées dans CLAUDE.md §4.

## 2026-11-12 — Lot P0 : audit des données

- Fait : inventaire des sources (transport.data.gouv.fr, jeu « ametis »), téléchargement des deux GTFS et d'instantanés des 4 flux GTFS-RT (fichiers dans le scratchpad de session, hors dépôt), analyse par script Python sans dépendance.
- Livrable : `docs/P0-audit-donnees.md`. `spec.md` : P0.1 et P0.2 cochés, P0.3 coché sauf « couverture réelle » (partiel, un seul instantané).
- Constats clés : licence `lov2` (vérifiée via l'API data.gouv.fr, une recherche web disait ODbL : écarté) ; deux GTFS sans aucun identifiant commun, arrêts rapprochables par coordonnées (740/740 à < 15 m) ; GTFS pour GTFS-RT valide 2 semaines, sans tracés ni couleurs ; Service Alerts vide → perturbations **bloquées** ; Trip Updates avec heures non croissantes (E022) ; flux scolaire en JSON non conforme ; en-tête RT mis à jour toutes les 40-60 s.
- Non vérifié : couverture soir/week-end/vacances ; fréquence mesurée sur 3 min seulement.
- Incident : une requête Python avec un User-Agent modifié a été refusée par le contrôle automatique ; refait avec `curl` sans modification.
- Décisions du propriétaire (chat, suite P0) : packages NuGet Identity EF, Google.Protobuf, StackExchange.Redis approuvés ; frontend approuvé avec **Tailwind CSS au lieu de Vuetify** (écart à la spec P1.4 « Configurer Vuetify ») ; modèle à deux GTFS + correspondance ; géographie SQL Server. Reporté dans CLAUDE.md §4.

## 2026-11-12 — Lots P1 à P4 (MVP) et socle P3/P8 : développement

Fait :
- Backend (`AmetisAPI\AmetisAPI\src`) : domaine GTFS/réseau/temps réel/comptes ; cas d'usage Core (import GTFS versionné, réseau, horaires théoriques, temps réel, favoris, compte, santé des données) ; Infrastructure (lecteur CSV RFC 4180, lecteur GTFS zip, décodeur GTFS-RT protobuf sans code généré, dépôts EF + SqlBulkCopy, Redis/mémoire, client HTTP) ; API (contrôleurs lines, stops, vehicles, alerts, feeds, favorites, accounts ; Identity sous /api/v1/auth ; hub SignalR /hubs/vehicles ; contrôle de santé « Données Ametis ») ; nouveau projet `AmetisAPI.Worker` (imports toutes les 6 h, collecte GTFS-RT 30 s, purge 30 jours).
- Base : 3 schémas (gtfs, rt, app) + tables Identity dans `AmetisAPI.Database` (DDL généré depuis le modèle EF, index spatial sur les arrêts). Template : `AmetisAPI.APP` (MAUI) supprimé (décision du propriétaire) ; préfixe d'erreurs `to.do.` → `ametis.` ; codes `ametis.gtfs.*` ajoutés.
- Import : chaque import crée une version ; contenu identique (SHA-256) = version « Skipped », rien n'est réécrit ; échec = version « Failed » purgée, version active conservée ; résumé des lignes/arrêts ajoutés et supprimés dans le journal ; rejets consignés (fichier, ligne, raison) ; seuil d'échec à 5 % d'horaires rejetés.
- Correspondance temps réel → réseau : arrêts par distance (< 15 m + même nom = haute, < 15 m = moyenne, < 50 m = basse), lignes par nom court (NAM1 → M1 confirmé par l'identifiant Ametis `AMI:NAM1-1`).
- Frontend `frontend\` : Vite 8, Vue 3.5, TypeScript 6, Tailwind 4, Pinia 4, Vue Router 5, MapLibre 6, SignalR 11, Vitest 5. Pages accueil (recherche, arrêts proches, favoris, perturbations), lignes, ligne, arrêt (temps réel + théorique), carte live, connexion, inscription, compte, état des données. Thèmes clair/sombre.
- Docker : Dockerfiles API/Worker/frontend (nginx), `docker-compose.yml` à la racine. **Non vérifiés** (Docker absent).

Corrections de failles du template (signalées en §9 « à approuver ») — appliquées car l'ajout des comptes les rendait exploitables, à valider par le propriétaire :
- `HandleExceptionMiddleware` : renvoie un problème HTTP générique (code `ametis.unknown_error`) ; test écrit d'abord, vu en échec, puis corrigé.
- CORS : origines limitées à `Cors:AllowedOrigins` (au lieu de toute origine avec cookies).
- `AppDbContext` : `EnableSensitiveDataLogging()` retiré (risque de journaliser des e-mails) ; toujours actif dans la fabrique de contexte des tests.

Autres modifications de l'existant :
- `ADbUseCase.WithDatabaseAsync` ajouté (factorise attente/libération du sémaphore).
- `BaseController.SendAsync` ajouté (validation FluentValidation + médiateur + presenter) ; `ValuePresenter<T>`, `ValueResponse<T>` génériques.
- `InfrastructureModuleTests` : liste des types attendus complétée avec les 9 nouveaux enregistrements (règle inchangée, pas affaiblie).
- `DbContextFactory` (tests) : `UseNetTopologySuite()` ajouté (colonne geography).
- Dates EF relues en UTC (convertisseur global) : défaut constaté en direct (dates sans « Z »).
- Packages ajoutés (approuvés) : Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.7, Google.Protobuf 3.36.2, StackExchange.Redis 3.4.31 ; FrameworkReference Microsoft.AspNetCore.App dans Infrastructure. Worker : mêmes versions Autofac/Cortex/Serilog que l'API.

Vérifié (sortie à l'appui) :
- `dotnet build AmetisAPI.slnx` : 0 erreur ; avertissements uniquement NuGet (NU1902/NU1903 vulnérabilités transitives préexistantes, NU1510).
- `dotnet test AmetisAPI.slnx --settings test.runsettings --collect:"XPlat Code Coverage" --blame-crash` (DACPACPATH renseigné, LocalDb) : API 2/2, Core 46/46, Infrastructure 45 réussis + 5 ignorés (ignorés préexistants du template). 0 échec.
- Frontend : `vue-tsc -b` sans erreur ; `vitest run` 8/8 ; `vite build` OK.
- Données réelles (base de développement LocalDb `AmetisDev`, jamais la production) : GTFS complet 42 lignes / 1 452 arrêts / 7 174 courses / 239 531 horaires, 0 rejet ; GTFS temps réel 22 / 740 / 5 049 / 172 822, 0 rejet ; 740/740 arrêts rattachés (734 haute confiance), 22/22 lignes. Collecte : ~80 véhicules, historique alimenté sans doublon. Endpoints testés par curl (400, 401, 404, favoris, suppression de compte en cascade). Interface vérifiée dans le navigateur (arrêt, carte live, ligne, largeur mobile 375 px).
- Défaut trouvé par test et corrigé : message de rejet dépendant de la culture (« 2,3 »).

Non vérifié / limites :
- Redis réel, Worker séparé + Redis, Docker, HTTPS de production, fond MapTiler (pas de clé).
- E-mail (confirmation, mot de passe oublié) : bloqué, SMTP non choisi ; `RequireConfirmedEmail = false`.
- Politique de mot de passe : défauts d'Identity (point ouvert §9 non tranché).
- DACPAC ciblant SQL Server 2025 (Sql170) déployé sur LocalDb 2019 avec `AllowIncompatiblePlatform` (comme les tests du template).
- Sémaphore statique par cas d'usage (`ADbUseCase`) : sérialise les lectures concurrentes d'un même type de requête ; à mesurer en charge (P9).
- Couverture temps réel : un seul jour observé ; prochains passages temps réel au terminus affichés (arrivées), non filtrés.

## 2026-11-12 — Fond de carte gratuit et e-mails de compte

Demande du propriétaire : pas de clé de carte, trouver une alternative gratuite ; SMTP configuré par lui dans appsettings.

Fait :
- Carte : MapTiler remplacé par OpenFreeMap (gratuit, sans clé ni inscription, données OpenStreetMap) : styles `liberty` (clair) et `dark` (sombre), police des étiquettes alignée sur « Noto Sans Bold » fournie par OpenFreeMap. `VITE_MAPTILER_KEY` retiré (env.d.ts, Dockerfile, docker-compose). Fichiers : `frontend/src/components/MapPanel.vue`, `frontend/src/env.d.ts`, `frontend/Dockerfile`, `docker-compose.yml`. Décision reportée dans CLAUDE.md §4 et spec P3.1.
- E-mails : `SmtpOptions` (section `Smtp` : Host, Port, EnableSsl, UserName, Password, FromAddress, FromName, PublicUrl, PickupDirectory), `SmtpEmailSender` (System.Net.Mail, aucun package ajouté) remplaçant l'expéditeur vide d'Identity, `AccountEmailTemplates` (textes français, liens vers les pages du site, jamais d'adresse e-mail dans un lien). `RequireConfirmedEmail` = section Smtp renseignée. Avertissement au démarrage si Smtp vide. Placeholders vides ajoutés à `appsettings.json` (aucun secret versionné ; mot de passe à mettre dans un appsettings non versionné ou la variable `Smtp__Password`).
- Messages d'Identity traduits (`FrenchIdentityErrorDescriber`) : ils sortaient en anglais.
- Frontend : pages `/confirmation-email`, `/mot-de-passe-oublie`, `/mot-de-passe/nouveau` ; connexion : messages « adresse non confirmée » et « compte bloqué », renvoi de l'e-mail de confirmation, lien « Mot de passe oublié ? » ; inscription : message de confirmation si l'adresse doit être validée ; compte : mention « non confirmée ».

Défauts trouvés et corrigés (test d'abord) :
- Identity transmet le lien de confirmation déjà encodé en HTML ; il était réencodé (`&amp;amp;`), le paramètre `code` était perdu (erreur 500 à la confirmation). Test `Confirmation_Link_Points_To_Site_Page_With_Identity_Parameters` mis à jour avec le lien réel, vu en échec, puis corrigé (décodage avant reconstruction).
- Mode dossier de dépôt : SSL incompatible, désactivé dans ce mode seulement.

Vérifié :
- `dotnet test AmetisAPI.slnx --settings test.runsettings --collect:"XPlat Code Coverage" --blame-crash` : API 2/2, Core 46/46, Infrastructure 50 réussis + 5 ignorés (préexistants). 0 échec. Build : 0 erreur.
- Frontend : `vue-tsc -b` sans erreur, `vitest run` 8/8, `vite build` OK.
- Parcours réel sur la base de développement, e-mails écrits en fichiers (`Smtp:PickupDirectory`) : inscription → connexion refusée (NotAllowed) → lien reçu vers la page du site → confirmation 200 → connexion 200 → mot de passe oublié → code → réinitialisation 200 → nouveau mot de passe accepté, ancien refusé. Aucune adresse e-mail dans les logs. Comptes de test supprimés ensuite.
- Carte : styles et tuiles OpenFreeMap chargés, fond sombre affiché avec les bus en direct et l'attribution OpenStreetMap.

Non vérifié :
- Envoi réel par un serveur SMTP (dépend de la configuration du propriétaire).
- Derrière un reverse proxy, `Smtp:PublicUrl` doit être l'adresse publique du site, sinon les liens pointent vers l'API.
- Conditions d'usage d'OpenFreeMap à relire avant la mise en production (service gratuit, sans garantie de disponibilité).

## 2026-11-12 — Infos trafic, déviations, notifications push, bus en direct détaillés

Demande : espace dédié aux évènements (lignes déviées…), abonnement aux notifications en direct, voir les déviations et le sens des bus sur la carte, avance/retard des bus en direct.
Décisions du propriétaire : alertes officielles **et** déviations estimées ; Web Push complet.
Constat : flux Service Alerts toujours vide ; ametis.fr renvoie 403 aux accès automatisés (robots.txt permissif) : non contourné, aucune lecture du site.

Fait :
- Bus en direct (`GetVehiclesUseCase`) : avance/retard et prochain arrêt tirés des Trip Updates (prochain arrêt non dépassé de la course), état hors itinéraire et tracé des positions hors itinéraire.
- Détection de déviation (`DeviationDetector`, `GeoMath`) : bus rattaché avec confiance ≥ moyenne, position fraîche, à plus de 150 m de tous les tracés de sa ligne sur 3 positions successives (horodatages distincts). État conservé dans l'instantané temps réel (`RouteStates`).
- Évènements (`rt.NetworkEvent`, `NetworkEventRepository`) : alertes officielles (closes dès qu'elles quittent le flux) et déviations détectées (closes après 10 min sans détection) ; confiance faible (1 bus) ou moyenne (≥ 2 bus, ou un bus hors tracé sur 6 positions). `ProcessNetworkEventsUseCase` lancé après chaque collecte. Purge des évènements clos depuis plus de 30 jours avec l'historique.
- Web Push sans package : `WebPushEncryptor` (RFC 8291, aes128gcm), `VapidKeys` (JWT ES256, RFC 8292), `WebPushSender` ; abonnements `app.PushSubscription` (cascade avec le compte, index unique sur le hachage de l'adresse) ; liste blanche des services push (FCM, Mozilla, WNS, Apple) contre l'usage du serveur pour appeler une adresse arbitraire ; abonnement expiré (404/410) supprimé. Commande `dotnet run --project src/AmetisAPI.API -- generate-vapid-keys`. Section `Push` (vide) dans appsettings.json.
- API : `GET /api/v1/events`, `GET /api/v1/notifications/key` (503 si non configuré, code `ametis.push.disabled`), `PUT /api/v1/notifications/subscription`, `POST /api/v1/notifications/unsubscribe` ; SignalR : événement `events`.
- Frontend : page `/infos-trafic` (en cours, 7 derniers jours, activation des notifications « toutes les lignes » / « lignes favorites »), compteur dans le menu, résumé sur l'accueil ; service worker `public/sw.js`, manifeste et icône ; carte : pastille avec pointe orientée selon le cap (sans cap : pas de flèche), contour rouge et tracé pointillé rouge pour les bus hors itinéraire, légende ; fiche bus (retard coloré, prochain arrêt, vitesse, hors itinéraire) ; page ligne : filtre des bus par sens, liste des bus en circulation, perturbations de la ligne.
- Schéma : tables `rt.NetworkEvent` et `app.PushSubscription` ajoutées au DACPAC (DDL généré depuis EF), déployé sur la base de développement.
- `InfrastructureModuleTests` : 3 nouveaux enregistrements ajoutés à la liste attendue.

Vérifié :
- `dotnet test AmetisAPI.slnx --settings test.runsettings` : API 2/2, Core 69/69, Infrastructure 56 réussis + 5 ignorés (préexistants). 0 échec. Build : 0 erreur.
- Chiffrement Web Push : sortie identique octet pour octet à l'annexe A de la RFC 8291 ; jeton VAPID vérifié avec la clé publique.
- Frontend : `vue-tsc -b` sans erreur, `vitest run` 9/9, `vite build` OK.
- Données réelles (base de développement) : retard et prochain arrêt pour 59 bus sur 73 ; déviation détectée sur la ligne 11 (un bus à ~300 m du tracé sur 6 positions, confiance moyenne) affichée sur la page Infos trafic et la page de la ligne ; abonnement : adresse interne refusée (400), adresse FCM acceptée puis désinscrite ; compte de test supprimé.

Non vérifié :
- Réception réelle d'une notification push : les notifications sont bloquées dans le navigateur intégré ; à tester sur un vrai navigateur avec des clés VAPID configurées. Sur iPhone, l'application doit être installée sur l'écran d'accueil.
- La déviation de la ligne 11 n'est pas confirmée : le seuil (150 m, 3 positions) peut produire de fausses alertes (tracé GTFS incomplet, terminus, dépôt). À observer sur plusieurs jours avant d'ajuster.
- Les clés VAPID utilisées pour l'essai étaient éphémères (variables d'environnement) : en générer et les configurer de façon permanente, sinon les abonnements deviennent invalides à chaque changement de clé.

## 2026-11-12 — Trajet d'un bus, heures prévues et effectives partout

Demande : au clic sur un bus, voir son trajet (arrêts desservis et à venir, horaires prévus, heures effectives, retard) et, sur tous les arrêts, l'horaire prévu et l'heure effective avec l'avance ou le retard.

Analyse des données (flux du jour) : les Trip Updates Ametis listent **tous** les arrêts de chaque course, dans l'ordre, **sans** `stop_sequence` ni `start_date` ; pour les arrêts déjà desservis l'heure publiée est l'heure de passage, pour les suivants une estimation ; heure publiée − retard = horaire du GTFS temps réel à la seconde près (vérifié sur la course 2-11-1-B-132700, 49 arrêts).

Fait :
- `rt.StopPassage` (DACPAC, déployé en développement) et `StopPassageRepository` : chaque collecte enregistre ou met à jour (MERGE, clé course + arrêt + heure prévue) les passages publiés ; rang = ordre du flux ; sans retard publié, rien n'est enregistré (heure prévue inconnue). Purge à 30 jours avec l'historique.
- `GET /api/v1/trips/{tripId}` (`GetTripUseCase`) : arrêts avec heure prévue, heure effective, écart, statut desservi / prochain / à venir, position du bus et trace de ses positions enregistrées.
- `GET /api/v1/stops/{id}/board` (`GetStopBoardUseCase`) : horaires théoriques du réseau complétés par l'heure effective quand la course est suivie (rapprochement même ligne et heures prévues à ± 90 s, faute d'identifiant commun entre les deux GTFS) ; passage temps réel sans horaire théorique ajouté avec son heure prévue ; terminus exclus des prochains départs ; « derniers passages » sur 30 minutes. Calcul des départs théoriques factorisé (`DepartureCalculator`).
- Règle unique « un arrêt est desservi dès que son heure effective est dépassée » (`RealtimeRules.IsPassed`) pour la fiche du bus, le détail de la course et le tableau d'arrêt. `VehicleView` porte désormais `TripId`.
- Frontend : composant `TimeDelay` (heure prévue barrée si écart ≥ 1 min, heure effective, avance/retard en couleur, « théorique » sinon) utilisé sur la page d'arrêt (prochains et derniers passages) et la nouvelle page `/courses/:tripId` (frise des arrêts, prochain arrêt mis en avant et centré, carte avec la trace du bus, rafraîchie toutes les 30 s) ; lien « Voir le trajet » sur la fiche d'un bus (carte, page ligne).

Défauts trouvés en essai réel et corrigés (test d'abord, vu en échec puis corrigé) :
- Un bus venant de passer restait dans « prochains passages » avec son horaire théorique (`Bus_Just_Passed_Removes_Its_Scheduled_Departure_From_Upcoming`).
- La fiche du bus annonçait comme prochain un arrêt passé depuis moins de 60 s, en contradiction avec la frise (`Next_Stop_Is_The_First_Not_Yet_Passed_In_Feed_Order`).

Vérifié :
- `dotnet test AmetisAPI.slnx --settings test.runsettings --collect:"XPlat Code Coverage" --blame-crash` : API 2/2, Core 75/75, Infrastructure 58 réussis + 5 ignorés (préexistants). 0 échec. Build : 0 erreur.
- Frontend : `vue-tsc -b` sans erreur, `vitest run` 9/9, `vite build` OK.
- Données réelles (base de développement) : 2 922 passages de 86 courses dès la première collecte ; détail de courses des lignes 7 et L cohérent ; tableau de Gare du Nord : 25 passages à venir dont 18 avec heure effective, aucun doublon entre passés et à venir ; pages vérifiées dans le navigateur.

Limites :
- Les heures des arrêts desservis sont celles publiées par Ametis, pas une mesure GPS indépendante.
- Le flux contient des heures non croissantes (anomalie E022 déjà connue) : affichées telles quelles.
- La trace du bus n'a qu'un point toutes les 40 à 60 s (fréquence du flux) : tracé en segments droits.
- Les courses non publiées en temps réel (TAD, scolaires) restent en horaire théorique seul.

## 2026-11-12 — Trajet du bus affiché directement sur la carte

Demande : ne pas passer par le lien « Voir le trajet » mais afficher le trajet du bus cliqué directement sur `/carte`, avec les horaires sur les arrêts concernés.

Fait :
- API : `TripStopView` porte la position de chaque arrêt (`RealtimeStopInfo`, `IRealtimeReferenceRepository.GetRealtimeStopsAsync`).
- `MapPanel` : nouvelles couches « trace du trajet » (positions enregistrées), « arrêts de la course » (plein = desservi, contour vert = prochain, blanc = à venir) et étiquettes « nom / heure effective (prévu HH:MM, écart) » colorées selon l'écart (mêmes seuils que le texte), priorité d'affichage au prochain arrêt puis aux arrêts à venir ; les autres bus sont estompés et leurs numéros masqués ; cadrage sur la course à sa sélection seulement.
- Page carte : un clic sur un bus affiche son trajet sur la carte (rafraîchi toutes les 30 s), la fiche du bus sous la carte sans lien de navigation, bouton « Fermer » ; les arrêts du réseau sont masqués pendant l'affichage d'un trajet.
- Défaut trouvé en essai et corrigé : un clic sur un bus posé sur un arrêt ouvrait la page de l'arrêt ; un seul gestionnaire de clic donne désormais la priorité aux bus.
- La page `/courses/:tripId` reste accessible depuis les tableaux d'arrêt et la page ligne.

Vérifié :
- `dotnet test AmetisAPI.slnx --settings test.runsettings --collect:"XPlat Code Coverage" --blame-crash` : API 2/2, Core 75/75 (test du détail de course étendu aux positions), Infrastructure 58 + 5 ignorés (préexistants). 0 échec.
- Frontend : `vue-tsc -b` sans erreur, `vitest run` 12/12 (libellés d'arrêt et couleurs ajoutés), `vite build` OK.
- Navigateur, données réelles : clic sur un bus N1 puis un bus 6 : arrêts desservis « passé 14:49 (prévu 14:44, retard 6 min) », prochain arrêt mis en avant, arrêts à venir avec heure estimée.

Limite : sur les zones denses, certaines étiquettes sont masquées par la carte pour éviter les chevauchements (elles réapparaissent en zoomant).

## 2026-11-12 — Renommage « Nautibus » et déploiement Docker pour LXC Proxmox

Demande : docker compose + script de déploiement sur LXC Proxmox ; renommer l'application avec un jeu de mots autour de Jules Verne (amiénois). Choix du propriétaire : **Nautibus** (Nautilus + bus), accroche « le tour d'Amiens en temps réel ».

Fait :
- Renommage des textes visibles uniquement : titre et description de la page, manifeste (nom, nom court), icône « N », en-tête, pied de page (« non affiliée à Ametis ni à Keolis »), notifications, e-mails (expéditeur par défaut « Nautibus »). Les projets et espaces de noms `AmetisAPI.*` sont conservés. `SmtpEmailSenderTests` adapté au nouveau nom d'expéditeur (règle inchangée).
- API : option `ReverseProxy:Enabled` (en-têtes X-Forwarded-For/Proto/Host lus, redirection HTTPS laissée au proxy). Sans elle, derrière le proxy, la limitation des connexions aurait vu tous les visiteurs comme une seule adresse.
- `docker-compose.yml` (racine) : caddy (seul service exposé, 80/443) → web (nginx, fichiers du site) et api ; worker ; sqlserver 2022 édition **Express** (gratuite en production) ; redis sans persistance ; `db-init` (profil `init`) publiant le DACPAC avec sqlpackage, sans perte de données. Journaux en fichier (volume) et en console.
- `deploy.sh` : `install` (vérifie x86_64, mémoire, Docker et options LXC nesting/keyctl ; propose d'installer Docker ; pose les questions domaine/IP, e-mail, SMTP facultatif ; génère `.env` en 600 avec mots de passe aléatoires, clés VAPID, Caddyfile ; construit, déploie le schéma, crée le compte SQL applicatif limité à db_datareader/db_datawriter, démarre et attend la réponse de l'API), `update` (sauvegarde puis mise à jour), `backup`, `status`, `logs`, `stop`.
- Dockerfiles API et Worker : SDK épinglé sur 10.0.201 (global.json), image aspnet pour les deux (le Worker dépend du framework ASP.NET Core via l'infrastructure). `.dockerignore` ajoutés. `frontend/nginx.conf` réduit aux fichiers statiques.
- `deploy/schema/AmetisAPI.Database.dacpac` (copie du DACPAC construit) et `deploy/update-dacpac.ps1` pour le régénérer après un changement de schéma. `.gitignore` : `.env`, `backups/`, `deploy/Caddyfile`.

Vérifié :
- `dotnet test AmetisAPI.slnx --settings test.runsettings` : API 2/2, Core 75/75, Infrastructure 58 + 5 ignorés (préexistants). 0 échec. Build 0 erreur.
- Frontend : `vue-tsc -b`, `vitest run` 12/12, `vite build` OK.
- `bash -n deploy.sh` : syntaxe correcte ; fichiers en fins de ligne LF.

Non vérifié (Docker absent du poste) :
- Aucune image construite ni conteneur lancé ; `docker-compose.yml` relu mais non validé par un analyseur YAML ni par `docker compose config`.
- Existence de l'image `mcr.microsoft.com/dotnet/sdk:10.0.201`, fonctionnement de sqlpackage en outil dotnet dans le conteneur, présence de tzdata dans l'image aspnet.
- HTTPS : Let's Encrypt exige un domaine pointant sur le serveur et les ports 80/443 joignables ; sur IP locale, certificat interne de Caddy (avertissement du navigateur ; les cookies de connexion exigent HTTPS).
- Sauvegardes non planifiées, restauration non testée.

## 2026-11-12 — Premier déploiement Docker réel (poste local) et corrections

Demande : générer le `.env` local avec des valeurs aléatoires pour l'URL nautibus.florent-dubut.fr (le `docker compose up` du propriétaire échouait faute de `.env`).

Fait :
- `.env` généré (mots de passe SA et applicatif aléatoires, clés VAPID générées par l'API, SMTP vide) et `deploy/Caddyfile` (domaine nautibus.florent-dubut.fr + https://localhost en certificat interne pour l'essai local). Contact VAPID = URL du site (aucune adresse e-mail transmise aux services push). Secrets non reproduits ici.
- `docker-compose.yml` rendu autonome : `db-init` (schéma) n'est plus dans un profil et un service `db-user` crée/met à jour le compte SQL applicatif ; api et worker attendent leur réussite. `docker compose up -d --build` suffit. `deploy.sh` adapté. Le bloc réutilisable du Caddyfile est désormais écrit avant les sites qui l'importent.
- Ajouts du propriétaire constatés et conservés : persistance des clés de protection des données (`DataProtection:KeysPath`, volume `dpkeys`).

Défauts trouvés au premier démarrage réel et corrigés :
- Worker : Mapster non enregistré (l'API le faisait) → aucun cas d'usage instanciable, imports et collecte jamais exécutés. Enregistrement ajouté comme dans l'API.
- Un import GTFS en échec (coupure réseau constatée sur le GTFS temps réel) n'était retenté que 6 h plus tard : nouvel essai après 10 min (`ScheduledMediatorService.NextDelay`, `GtfsImportService`), test `Failed_Import_Is_Retried_After_Ten_Minutes_Then_Back_To_Six_Hours`.
- Construction : sous Docker Desktop sur ce poste, les constructions parallèles saturent le relais DNS (NU1301 sur api.nuget.org, `npm ci` bloqué) ; construites une par une, elles passent. Particularité du poste, non corrigée dans les fichiers.

Vérifié (Docker Desktop local, 2026-11-12) :
- Construction des 3 images ; `docker compose up -d` : sqlserver et redis sains, schéma publié par sqlpackage (« Successfully published database »), compte applicatif créé, tous les services démarrés.
- Worker : réseau importé (42 lignes, 1 452 arrêts, 0 rejet), GTFS temps réel importé à la nouvelle tentative, santé « Healthy ».
- À travers Caddy en HTTPS (https://localhost) : site 200 ; 102 bus dont 96 rattachés à une ligne et 86 avec retard ; tableau Gare du Nord 25 passages dont 15 en temps réel ; inscription, connexion (cookie Secure), compte, suppression ; clé push 200 ; limitation des connexions déclenchée (429) sur l'adresse réelle du client.
- `dotnet test AmetisAPI.slnx` : API 2/2, Core 76/76, Infrastructure 58 + 5 ignorés (préexistants). 0 échec.

Non vérifié :
- Certificat Let's Encrypt : nautibus.florent-dubut.fr n'existe pas encore dans le DNS (NXDOMAIN). Il faut un enregistrement A vers l'IP publique et les ports 80/443 redirigés vers le LXC.
- Déploiement sur le LXC Proxmox lui-même (`deploy.sh` non exécuté sur Linux).

## 2026-11-12 — HTTPS confié à Nginx Proxy Manager (suppression de Caddy)

Demande : le serveur dispose d'un Nginx Proxy Manager (NPM) qui gère HTTPS. Décision du propriétaire : HTTPS et certificats par NPM, plus de proxy HTTPS dans la pile.

Fait :
- `docker-compose.yml` : service `caddy` et volumes `caddy_data`/`caddy_config` supprimés. Le service `web` publie seul un port HTTP (`${HTTP_BIND:-0.0.0.0}:${HTTP_PORT:-8080}:80`), cible du proxy host NPM.
- `frontend/nginx.conf` : nginx du service web devient le point d'entrée unique (fichiers du site, relais de `/api`, `/hubs` en WebSocket, `/health` vers `api:8080`, gzip). Les en-têtes `X-Forwarded-For`/`X-Forwarded-Proto` reçus de NPM sont transmis tels quels (repli sur l'adresse de connexion s'ils sont absents) : avec la limite par défaut d'un saut, l'API retient l'adresse du client vue par NPM. Sans cela, la limitation des connexions aurait vu NPM comme seul client. En-têtes de sécurité (nosniff, Referrer-Policy) dans `frontend/nautibus-headers.conf`, inclus dans chaque bloc (les `add_header` d'un bloc remplacent ceux du serveur). `frontend/Dockerfile` copie ce fichier.
- `deploy.sh` : génération du Caddyfile supprimée ; `install` demande le domaine (obligatoire), le port HTTP (8080 par défaut) et un e-mail push facultatif (sinon l'URL du site sert de contact VAPID) ; `upgrade_env` ajoute `HTTP_PORT`/`HTTP_BIND` aux `.env` existants ; santé vérifiée à travers nginx du service web ; instructions NPM en tête du script et en fin d'installation.
- `deploy/Caddyfile` supprimé, entrée retirée de `.gitignore`. Commentaire de `Program.cs` mis à jour (aucun changement de code).

Réglage NPM attendu : proxy host `nautibus.florent-dubut.fr` → `http://<IP du LXC>:8080`, « Websockets Support » activé, certificat Let's Encrypt, « Force SSL », « HSTS Enabled ».

Vérifié (Docker sous WSL, NPM simulé par curl avec X-Forwarded-For/Proto) :
- `docker compose config --quiet` OK ; `nginx -t` OK (nginx:1.27-alpine) ; `bash -n deploy.sh` OK.
- Pile construite et démarrée : site 200 avec en-têtes de sécurité, route SPA 200, `/health` Healthy, négociation SignalR puis WebSocket 101, connexion : 10 essais puis 429 pour l'adresse A, l'adresse B reste autorisée (401) → adresse réelle du client bien vue par l'API. Pile arrêtée ensuite (`docker compose down`, volumes conservés).

Non vérifié / limites :
- NPM réel et certificat non testés.
- L'API se fie aux en-têtes X-Forwarded-* reçus sur le port publié : un accès direct à ce port (sans NPM) peut les falsifier. Restreindre `HTTP_BIND` ou le pare-feu à NPM si possible.
- Préexistant, non corrigé : `UseHsts()` est appelé avant `UseForwardedHeaders()`, l'API n'émet donc pas HSTS derrière un proxy ; sans effet si « HSTS Enabled » est coché dans NPM.

## 2026-11-12 — Carte de production invisible : type MIME du worker MapLibre

Signalement : fond de carte absent en production.

Diagnostic sur `https://nautibus.florent-dubut.fr/carte` :
- L'application charge la page, les véhicules temps réel et le style OpenFreeMap. L'endpoint OpenFreeMap répond en HTTPS.
- Console navigateur : `Error: Worker failed to load. Check that the worker URL is correct.` Le bundle demande `/assets/maplibre-gl-worker-Boc9pDRP.mjs`; cette URL renvoie 200 mais `Content-Type: application/octet-stream`. Avec le contrôle MIME des module workers (et `X-Content-Type-Options: nosniff`), le navigateur refuse le script, ce qui empêche le rendu du fond.

Correction dans `frontend/nginx.conf` : bloc spécifique pour les `.mjs` avec `default_type application/javascript`, conservation des en-têtes de sécurité et du cache immuable. Aucun changement de code MapLibre ou de service externe.

Vérifié :
- Image frontend reconstruite.
- `nginx -t` : succès.
- Requête HTTP locale sur le worker : 200, `Content-Type: application/javascript`, `X-Content-Type-Options: nosniff`, cache immuable.
- Navigateur sur la build locale : fond OpenFreeMap visible (rendu cartographique confirmé). La pile locale a été arrêtée ensuite avec `docker compose down`, volumes conservés.

À faire pour la production : reconstruire/redéployer l'image `web` (`docker compose build web && docker compose up -d web`) et recharger la page sans cache (Ctrl+F5). La correction n'est pas encore déployée sur le serveur de production.

Complément (cache navigateur) : le worker MapLibre était servi avec `Cache-Control: max-age=31536000, immutable` avant correction. Pour éviter qu'un navigateur conserve l'ancienne réponse MIME un an, `frontend/src/components/MapPanel.vue` ajoute `?v=module-mime-fix` à l'URL du worker. Image web reconstruite ; nginx renvoie le worker avec la nouvelle URL en 200 et `Content-Type: application/javascript`. Une session navigateur locale ne signale plus d'erreur de chargement du worker ; les requêtes API de cette vérification isolée échouaient en 502 faute d'API locale démarrée.
