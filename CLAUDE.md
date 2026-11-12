# CLAUDE.md — Nautibus (ex-Ametis Alternative)

Application web alternative pour le réseau de transport Ametis (Amiens Métropole) : consultation du réseau (lignes, arrêts, horaires théoriques), carte interactive, positions temps réel des véhicules, prédictions de passage, comptes utilisateurs avec favoris, puis routage multimodal, PWA et notifications.

Le template API .NET `AmetisAPI\Ametis\` (Clean Architecture) sert de base. La stack retenue (Autofac, MediatR, AutoMapper, SQL Server, Vue 3) diffère encore du template sur la médiation, le mapping et le frontend : voir §2 et §9.

## 0. Où est quoi

Racine du projet (`C:\Users\FCHS1795\Projets\Ametis`) :
- `spec.md` : backlog de développement V2 (lots P0 à P9, ordre recommandé, définition du MVP, règle de pilotage). **Source de vérité** du périmètre, de l'avancement (cases `- [ ]` / `- [x]`) et de l'ordre de travail.
- `AmetisAPI\Ametis\` : **code de l'API** (template). Solution `Ametis.slnx`, `global.json` (SDK 10.0.201), `.editorconfig`, `test.runsettings`, `CleanSln.ps1`, pipelines Azure DevOps dans `.azuredevops\`, `readme.md` du template.
- `CLAUDE.md` : le présent fichier.
- `history.md` : journal des modifications (voir §8).

Structure du template (`AmetisAPI\Ametis\`) :
- `src\Ametis.API` : ASP.NET Core (contrôleurs, Swagger, versionnage d'API, `/health`, CORS, Serilog, presenters, middleware d'erreurs).
- `src\Ametis.Core` : - `src\Ametis.Core` : cas d'usage et requêtes (MediatR).
- `src\Ametis.Domain` : interfaces, `DomainErrors`, entités.
- `src\Ametis.Infrastructure` : EF Core (`AppDbContext`), UnitOfWork, dépôts CRUD génériques, profils AutoMapper.
- `src\Ametis.Database` : projet SQL Server (`.sqlproj`, DACPAC).
- `src\Ametis.APP` : application MAUI du template. **Non retenue** : le frontend est en Vue 3. Ne pas la faire évoluer ; suppression à décider (§9).
- `tests\Ametis.{Core,Domain,Infrastructure,API}.Tests` : xUnit, NFluent, Moq, Bogus.

Ne pas modifier sans demande : `.vs\`, `bin\`, `obj\`, `dist\`. Les sorties de build et de couverture ne sont pas des sources.

Ne pas créer de fichier de planification ou de notes séparé : le suivi reste dans `spec.md`.

Claude n'a pas de mémoire d'une session à l'autre. Au début d'une session ou avant une nouvelle tâche, lire `spec.md` (lot en cours et lots dépendants), `history.md` s'il existe, puis le code concerné dans `AmetisAPI\Ametis\`, pour savoir où reprendre et ce qui a déjà été décidé ou écarté. Ne pas refaire ce qui est coché ; si c'est refait, écrire pourquoi.

## 1. Rôle

Tu agis comme développeur principal et architecte technique.
- Lire la spécification et le code existant avant de modifier quoi que ce soit.
- Privilégier la correction, la fiabilité des données et la traçabilité à la vitesse.
- Ne jamais affirmer qu'un build, un test ou un import a réussi sans preuve réelle.
- Signaler les hypothèses, incertitudes et limites.
- Si une règle manque ou se contredit (spec, code, source de données), demander avant d'inventer.

## 2. Stack

Stack **réelle du template** `AmetisAPI\Ametis\` (ne pas changer sans demande explicite) :
- **SDK** : .NET 10.0.201 (`global.json`, `allowPrerelease: false`). Projets `net10.0` ; `Ametis.APP` cible aussi `net10.0-windows10.0.19041.0` et Android/iOS/Mac Catalyst.
- **API** : ASP.NET Core, contrôleurs, Swagger (Swashbuckle 10.1.7), versionnage d'API par segment d'URL (`Asp.Versioning.Mvc.ApiExplorer`), `/health`, CORS.
- **Médiation** : **MediatR** (choix retenu). Les requêtes sont des `record` qui implémentent `IRequest<TResponse>` (le template utilise aujourd'hui `IQuery<TResponse>` de Cortex.Mediator, à migrer).
- **Injection de dépendances** : Autofac 9.1.0 (`Autofac.Extensions.DependencyInjection` 11.0.0). Les presenters sont enregistrés par convention (nom finissant par `Presenter`).
- **Validation** : FluentValidation 12.1.1.
- **Mapping** : AutoMapper (à la place de Mapster du template, à migrer).
- **Données** : EF Core 10.0.7 sur **SQL Server** (`UseSqlServer`, `Microsoft.EntityFrameworkCore.SqlServer.HierarchyId`, `...NetTopologySuite`), projet `Ametis.Database` (sqlproj / DACPAC). Connexion : clé `ConnectionStrings:Database` (placeholder `TODO EF Connection` à remplacer).
- **Logs** : Serilog (fichier `C:\Local\Ametis\Logs`, rétention 30 fichiers, enrichissement FromLogContext/MachineName/ThreadId).
- **Tests** : xUnit 2.9.3, NFluent 3.1.0, Moq 4.20.72, Bogus 35.6.5, runner `xunit.runner.visualstudio` 3.1.5. Tests d'infrastructure sur SQL Server / LocalDb (`LocalDbFixture`, `ASqlServerFixture`, `LaunchSettingsFixture`).
- **CI** : Azure DevOps (`.azuredevops\pipelines`), validation des titres de PR, SonarQube (`analyzeSonar.yml`).
- **Frontend** : **Vue 3 + TypeScript** (Vuetify, Pinia, Vue Router, MapLibre GL JS), projet à créer. Non présent dans le template.

Écarts entre le template et la stack retenue (à migrer, décisions du propriétaire) :
- Médiation : template = Cortex.Mediator ; retenu = MediatR.
- Mapping : template = Mapster ; retenu = AutoMapper.
- Frontend : template = MAUI (`Ametis.APP`) ; retenu = Vue 3 + TypeScript.
- Base : template = SQL Server ; retenu = SQL Server, sans écart. Le PostGIS de la spec est remplacé par les types spatiaux SQL Server (`NetTopologySuite` est déjà référencé).
- Authentification : spec = ASP.NET Core Identity ; readme du template = Azure AD en option. Non tranché.
- Autres exigences de la spec sans équivalent dans le template : Redis, SignalR, Worker GTFS-RT, OpenTripPlanner, Docker sur Proxmox.

Licences : MediatR et AutoMapper sont passés à une licence commerciale dans leurs versions récentes. Vérifier la licence applicable avant d'ajouter ou de monter de version (§9).

Interdit sans approbation préalable :
- Ajouter, remplacer ou monter de version un package (NuGet ou npm). Présenter d'abord la raison, les alternatives et les risques de licence.
- Changer la version du SDK, des frameworks cibles, des workloads MAUI ou la cible d'hébergement.
- Remplacer un composant de la stack (MediatR, AutoMapper, Autofac, Serilog, EF Core, SQL Server, Vue 3).
- Migrer le template vers la stack retenue (Cortex.Mediator vers MediatR, Mapster vers AutoMapper, MAUI vers Vue) sans demande de tâche explicite : c'est un chantier à part, à planifier.
- Harmoniser les versions de packages entre projets : elles ne le sont pas (ex. `Autofac` 9.1.0 et `Autofac.Extensions.DependencyInjection` 11.0.0, `Serilog.AspNetCore` 10.0.0 vs `Serilog` 4.3.1). Signaler, ne pas corriger sans demande.

## 3. Architecture

Clean Architecture (template), dépendances orientées vers le domaine uniquement :
- `Ametis.Domain` : entités, interfaces, `DomainErrors`. Aucune dépendance à EF Core, à SQL Server, à Redis ou à HTTP.
- `Ametis.Core` : cas d'usage (`AUseCase`, `ADbUseCase`, `Crud\`), requêtes et réponses, ports de sortie (`IOutputPort<T>`).
- `Ametis.Infrastructure` : `AppDbContext`, UnitOfWork, dépôts CRUD génériques, profils AutoMapper, accès aux données.
- `Ametis.API` : contrôleurs (héritent de `BaseController`), presenters (`AJsonPresenter`, `ValidationPresenter`), middleware d'erreurs, configuration.
- Frontend Vue 3 (dossier à créer, par exemple `frontend\`) : pages, composants, stores Pinia, services d'appel API, couche carte MapLibre isolée.

Ajouts demandés par la spec et absents du template, à placer ainsi (à valider avant P1.1) : hubs SignalR dans `Ametis.API`, Worker GTFS-RT et import GTFS dans un projet `Worker` séparé, client OTP et Redis derrière des interfaces du Domain/Core.

Règles :
- Pas de dépendance circulaire. Une classe = une responsabilité.
- Aucune logique métier ni accès direct aux données dans les contrôleurs, les presenters ou les ViewModels. Ils passent par un cas d'usage (`Core`).
- Un contrôleur ne fait que : recevoir la requête, l'envoyer au médiateur, la transmettre à un presenter.
- Les imports, la collecte temps réel et les calculs de prédiction ne bloquent jamais les requêtes utilisateur. Annulation, erreurs et progression remontent proprement.
- **Pas de duplication.** Avant d'écrire une fonction, une règle ou une classe, chercher ce qui existe déjà (`Core`, `Domain`, `Infrastructure`, extensions, `BaseController`, `AUseCase`, `ADbUseCase`) et le réutiliser ou le factoriser. Un motif répété trois fois devient une abstraction.
- Le routage OTP est toujours appelé via son abstraction ; aucun code ne dépend directement du client OTP.

### Conventions de nommage (`.editorconfig` du template)

- Méthodes asynchrones suffixées `Async`.
- Champs `static readonly` et privés : préfixe `m_` (`m_Factory`, `m_Secrets`).
- Paramètres de méthode : préfixe `p_` (`p_Job`, `p_Connections`).
- Variables locales : préfixe `v_` (`v_Report`, `v_Row`).
- Interfaces préfixées `I`. Membres publics en PascalCase.
- `var` déconseillé : types explicites.
- Une classe par fichier, nom du fichier = nom de la classe.
- Respecter `.editorconfig` (vérifier le niveau de chaque règle avant de s'en prévaloir).

### Patterns du template à suivre

- **Requêtes** : `record` implémentant `IRequest<TResponse>` (MediatR), nommées par l'action, avec un handler dédié. Le template actuel utilise `IQuery<TResponse>` (Cortex) : même forme, à migrer.
- **Cas d'usage** : hériter de `AUseCase<TRequest,TResponse>` (méthode `HandleLogic`). `ADbUseCase` ajoute un `SemaphoreSlim(1,1)` pour protéger la base : l'utiliser pour tout accès base.
- **Absence d'une ressource** : renvoyée dans la réponse sous forme d'`ErrorDto` (code de `DomainErrors`), pas levée comme exception.
- **Contrôleurs** : hériter de `BaseController` (route `api/v{version:apiVersion}/[controller]s`, JSON).
- **Réponses** : un presenter (`AJsonPresenter<T>`) transforme la réponse du cas d'usage en statut HTTP et JSON. Les erreurs de validation FluentValidation passent par `ValidationPresenter` (400 avec liste d'`ErrorDto`).

### Erreurs

- Les codes passent par le référentiel `Ametis.Domain\Errors\DomainErrors.cs`. Aucun code écrit en dur dans le code.
- Le préfixe des codes (`"to.do."`) est un **placeholder** du template : le remplacer par l'espace de noms du projet avant d'ajouter des codes (décision à prendre, §9).
- Un nouveau cas d'échec reçoit son code dédié, couvert par un test.
- L'API renvoie des erreurs structurées (problème HTTP standard). Aucune exception brute ni trace de pile n'est exposée.
- Les codes d'erreur publics sont un contrat : ne pas les renommer sans décision.

**Point du template à ne pas reproduire** : `HandleExceptionMiddleware` sérialise l'exception entière (`JsonSerializer.Serialize(v_Ex)`) dans la réponse, ce qui expose le détail interne et la pile. Le corriger avant la mise en service (renvoyer un problème HTTP générique, journaliser l'exception côté serveur), après accord.

## 4. Décisions figées

Tirées de `spec.md`. Ne pas les remettre en cause sans demande du propriétaire.

- **Données d'abord** : le lot P0 (audit des données Ametis) est terminé avant le développement fonctionnel. Il définit ce qui peut être affiché et prédit pour chaque service.
- **Sources** : GTFS statique pour le réseau, GTFS Realtime (Vehicle Positions, Trip Updates, Service Alerts) pour le temps réel. Toute donnée non vérifiée dans P0 n'est pas promise dans l'interface.
- **Une fonctionnalité dépendant d'une source Ametis indisponible est marquée « bloquée » ou « adaptée »**, jamais « terminée ».
- **Temps réel** : dernières positions dans Redis, diffusion par SignalR, historique persisté en base.
- **Rétention des positions GPS : 30 jours** (lot P8.1).
- **Routage** : OpenTripPlanner, isolé derrière une abstraction.
- **Carte** : MapLibre GL JS avec MapTiler, fonds clair et sombre.
- **Prédiction** : d'abord un modèle déterministe mesuré (P5.1-P5.2), puis un modèle statistique activé progressivement (P5.3). Chaque estimation porte sa provenance et un niveau de confiance.
- **Périmètre MVP** : import statique, consultation réseau, horaires théoriques, carte, positions véhicules si disponibles, premiers prochains passages, inscription/connexion, favoris, HTTPS sur Proxmox. Le reste (rejeu, notifications, hors ligne complet, administration approfondie) vient après.
- **Ordre des lots** : P0, P1, P2, P3, P4, P5, P6, P7, P8, P9. P4 et P6 peuvent avancer en parallèle s'ils ne bloquent pas les fondations.
- **Langue de l'interface** : français.

- **Décisions du propriétaire (2026-11-12)**, prioritaires sur §2 et §9 :
  - Médiation : **Cortex.Mediator** conservé (pas de MediatR). Mapping : **Mapster** conservé (pas d'AutoMapper). Raison : licences commerciales.
  - Authentification : **ASP.NET Core Identity + cookies** (HttpOnly, SameSite, CSRF).
  - `AmetisAPI.APP` (MAUI) : **à supprimer** ; frontend Vue 3.
  - Packages approuvés : `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.x, `Google.Protobuf`, `StackExchange.Redis` ; frontend Vite + Vue 3 + TS, **Tailwind CSS (à la place de Vuetify)**, Pinia, Vue Router, maplibre-gl, @microsoft/signalr, Vitest.
  - Données : import des **deux GTFS** (complet = référence réseau ; « pour GTFS-RT » = référence temps réel) avec table de correspondance arrêts/lignes et niveau de confiance (voir `docs/P0-audit-donnees.md`).
  - Géographie : types spatiaux SQL Server (`geography`, NetTopologySuite).
  - Fond de carte (2026-11-12) : **OpenFreeMap** (gratuit, sans clé), styles `liberty` (clair) et `dark` (sombre), à la place de MapTiler.
  - Infos trafic (2026-11-12) : alertes officielles Ametis **et** déviations **estimées** (bus à plus de 150 m de son tracé sur 3 positions successives), toujours présentées comme estimations. Le site ametis.fr refuse les accès automatisés (403) : pas de lecture du site.
  - Notifications : Web Push complet sans package (RFC 8291/8292 avec les primitives .NET), clés VAPID dans la section `Push`, abonnement par compte, « toutes les lignes » ou « lignes favorites ».
  - Nom de l'application (2026-11-12) : **Nautibus** (Nautilus + bus, clin d'œil à Jules Verne, amiénois), accroche « le tour d'Amiens en temps réel ». Seuls les textes visibles changent ; les projets et espaces de noms `AmetisAPI.*` sont conservés.
  - E-mails de compte : SMTP configuré dans la section `Smtp` d'appsettings (System.Net.Mail, sans package) ; confirmation d'adresse exigée seulement si `Smtp:Host` est renseigné.
  - Emplacement réel du template : `AmetisAPI\AmetisAPI\` (solution `AmetisAPI.slnx`, projets `AmetisAPI.*`).

## 5. Données : fiabilité avant tout

- Import GTFS **sans doublon** : une réimportation du même jeu de données ne crée ni doublon ni incohérence.
- Validation stricte de la structure des fichiers GTFS. Un fichier invalide ou incomplet fait échouer l'import proprement, sans écraser les données précédentes.
- Ne jamais tronquer, arrondir, remplacer ou supprimer silencieusement une valeur. Les enregistrements rejetés sont consignés avec la raison.
- Suppressions et modifications de réseau : tracées dans le journal d'import, jamais appliquées sans trace.
- Chaque import est journalisé (début, fin, statut, volume, erreurs) ; un import en échec ou incomplet est détectable depuis l'administration.
- GTFS-RT : vérifier la fraîcheur (horodatage du flux) ; une donnée obsolète est signalée comme telle, jamais affichée comme actuelle.
- Association véhicule ↔ course : toujours accompagnée d'un niveau de confiance. Une association ambiguë n'est pas affichée comme certaine.
- En cas d'échec d'un import ou d'une écriture : arrêt propre, conservation des lots validés, identification des écritures à résultat incertain. Ne pas rejouer aveuglément une opération qui peut créer des doublons ; vérifier l'état réel avant reprise.
- Les tests et validations ne se font **jamais** sur la base de production. Utiliser des fixtures GTFS, des jeux réduits, LocalDb ou SQL Server (fixtures du template : `LocalDbFixture`, `ASqlServerFixture`), ou SQL Server en conteneur via Testcontainers. Redis conteneurisé pour les tests temps réel.
- Le schéma de base passe par le projet `Ametis.Database` (DACPAC) et les migrations EF Core : ne jamais modifier le schéma de production sans script relu.

### Données personnelles (RGPD)

- Les favoris, préférences, comptes et positions liées à un utilisateur sont des données personnelles. Les minimiser, les isoler par utilisateur et les supprimer à la demande.
- Ne jamais écrire dans les logs, les erreurs ou les métriques : mots de passe, jetons, adresses e-mail, positions précises d'un utilisateur.
- Les données de positions de véhicules (GTFS-RT) et les positions GPS d'utilisateurs ne sont pas traitées de la même façon : voir les points ouverts.

## 6. Build, tests et non-régression

**Environnement** : commandes issues du template (`AmetisAPI\Ametis\`), **non exécutées à ce jour** sur cette machine. Si une commande ne peut pas être lancée, le dire et marquer le changement « non vérifié ». Ne jamais présenter comme vérifié ce qui ne l'a pas été.

Commandes (depuis `AmetisAPI\Ametis\`, sous Windows) :
- Nettoyage et restauration : `.\CleanSln.ps1` (supprime `bin` et `obj` des projets de `Ametis.slnx`, puis `dotnet restore`).
- Build : `dotnet build Ametis.slnx`.
- Tests (comme le pipeline) : `dotnet test Ametis.slnx --settings test.runsettings --collect:"XPlat Code Coverage" --blame-crash`.
- Tests d'infrastructure : exigent la variable `DACPACPATH` (chemin du DACPAC produit par `Ametis.Database`) et LocalDb ou SQL Server accessible.
- Un projet de test isolé : `dotnet test tests\<Projet>.Tests --settings test.runsettings`.
- Pour la validation de cette section, le relevé de sortie (nombre de tests passés et échoués, première erreur) est obligatoire.

Règles :
- Un changement n'est « validé » qu'avec build et tests verts, sortie à l'appui.
- **Avant de dire « terminé », lancer toute la suite** (`Ametis.slnx`), pas seulement les nouveaux tests. Un test existant qui échoue est une régression à corriger, jamais à contourner.
- **Correction d'un bug : le test d'abord.** Écrire un test qui reproduit le bug, le voir échouer, puis corriger. Le test reste dans la suite.
- Un test de comportement pour chaque règle métier ajoutée. Son nom décrit la règle.
- **Interdit** : supprimer, désactiver (`[Fact(Skip = ...)]`, `[Ignore]`), affaiblir une assertion, désactiver un analyseur ou une règle `.editorconfig` pour faire passer le build. Si une règle a changé et qu'un test est obsolète, le modifier et l'expliquer dans `history.md`.
- Tests rapides, indépendants et déterministes : pas d'ordre imposé, pas d'horloge ni d'aléa non maîtrisés (horloge injectée), **aucun accès réseau réel**. Les flux GTFS-RT, OTP, MapTiler, SMTP et Web Push sont simulés.
- Tests par couche : xUnit + NFluent (+ Moq, Bogus pour les données). Pour chaque fonctionnalité : cas nominal, cas limites (listes vides, bornes, enregistrements invalides, doublons), erreurs attendues avec leur code.
- Import GTFS : tester sur un jeu de fixtures couvrant services spéciaux, `calendar_dates`, `shapes` absent, arrêts orphelins.
- Prédictions : tests des métriques (MAE, RMSE, médiane, pourcentage à ±1/±2/±5 min) sur un jeu de validation figé, avec résultat attendu calculé à la main.
- Validation sur données réelles : autorisée pour les tests d'import et de collecte, en environnement de test, et doit être mentionnée dans le rapport.
- Résultat honnête : si des tests échouent ou n'ont pas pu être lancés, donner le nombre de tests passés et échoués, et la première erreur utile.
- Versions de packages de test non harmonisées entre projets (ex. `xunit.runner.visualstudio` 3.1.5 partout, mais `Microsoft.NET.Test.Sdk` à vérifier). Signaler, ne pas corriger sans demande.

## 7. Git et livraison

- Aucun dépôt Git trouvé dans `AmetisAPI\Ametis\` ni à la racine du projet. Le proposer en début de P1 (à confirmer), avec une branche principale et une branche de développement.
- CI : pipelines Azure DevOps du template (`.azuredevops\pipelines\dev-build-PR.yml`, `common-ci.yml`, templates `test-build-run.yml`, `database-build.yml`, `analyzeSonar.yml`). Confirmer la plateforme réelle avant de s'y fier (§9).
- Ne pas committer ni pousser sans demande explicite. Ne jamais réécrire l'historique.
- Ne jamais écraser des modifications non commitées avec `checkout`, `reset`, `stash` ou `clean` sans accord.
- Titres de PR : Conventional Commits (`type(scope)!: sujet`), validés par la CI (`validate-pr-title.yml`). Vérifier la liste exacte des types dans ce fichier avant d'écrire un titre.
- Environnements : Development, Preproduction, Production (P1.1). Pas de déploiement en production sans demande explicite.

## 8. Méthode de travail

1. Lire `spec.md` (lot concerné, critères d'acceptation, dépendances) et le code existant avant de coder.
2. Pour une tâche importante : lister les fichiers et impacts (architecture, sécurité, données), définir les tests, puis présenter le plan.
3. Changements incrémentaux, périmètre limité. Pas de refactorisation hors tâche. Distinguer les problèmes préexistants de ceux introduits.
4. Sous-agents seulement si le gain est réel : périmètres de fichiers disjoints, interfaces définies, intégration par le développeur principal.
5. Ne pas trancher seul une question ouverte (§9). La poser au propriétaire.
6. **Journal dans `history.md`** : à chaque modification, ajouter tout de suite une entrée datée en bas du fichier : ce qui a été fait, fichiers touchés, ce qui est vérifié et ce qui ne l'est pas, commande lancée et résultat, échecs avec le message utile, décisions et leur raison. Ajout seulement : une erreur se corrige par une nouvelle ligne. Ne jamais y mettre de secret, de chaîne de connexion ni de donnée personnelle (écrire « secret masqué »).
7. **Mettre à jour `spec.md`** quand une étape avance : cocher la case concernée. Si le comportement, le périmètre ou une règle change, le signaler dans la spec et dans `history.md`. Si le code et la spec se contredisent, le dire ; ne pas choisir en silence.

Rapport de fin de tâche : fichiers modifiés, tests lancés et résultat réel, risques restants, ce qui n'a pas pu être vérifié.

### Checklist avant de dire « terminé »

- [ ] Les critères d'acceptation du lot sont respectés et couverts par des tests.
- [ ] Build vert et **toute la suite** verte, sortie à l'appui ; sinon « non vérifié » ou « échec » avec la sortie.
- [ ] Aucun test supprimé, désactivé ou affaibli.
- [ ] Aucune duplication ajoutée ; aucune logique métier dans la couche présentation.
- [ ] Codes d'erreur dans le référentiel, couverts par un test.
- [ ] Aucune donnée personnelle ni secret dans les logs, erreurs ou rapports.
- [ ] Fonctionnalités dépendant d'une source non vérifiée marquées « bloquées » ou « adaptées ».
- [ ] `spec.md` mis à jour (cases cochées, changements de périmètre) ; `history.md` complété.

## 9. Points ouverts

À trancher par le propriétaire avant de les figer dans le code. **Les quatre premiers bloquent le lot P1** :
- **Licences MediatR et AutoMapper** : versions récentes sous licence commerciale pour un usage professionnel. Vérifier et choisir la version (ou l'alternative) avant de les ajouter au template : demande d'approbation obligatoire (§2).
- **Migration du template** : Cortex.Mediator vers MediatR, Mapster vers AutoMapper, MAUI (`Ametis.APP`) vers Vue 3. Décider si `Ametis.APP` est supprimé ou gardé hors du périmètre.
- **Authentification** : spec = ASP.NET Core Identity ; readme du template = Azure AD en option. Confirmer Identity, et le choix cookies ou JWT pour l'API et le SPA, et la politique de mot de passe.
- **Géographie en SQL Server** : PostGIS de la spec remplacé par les types spatiaux SQL Server. Confirmer que les besoins (recherche d'arrêts à proximité, tracés) sont couverts.
- **Redis et temps réel** : le template ne contient ni Redis, ni SignalR, ni Worker. Confirmer leur ajout et leur emplacement dans la solution.
- **Préfixe des codes d'erreur** : remplacer `"to.do."` dans `DomainErrors` (placeholder du template) par l'espace de noms Ametis.
- **Chaîne de connexion** : `ConnectionStrings:Database` = `TODO EF Connection` dans `appsettings.json`. Définir le gestionnaire de secrets (voir « Secrets et configuration » ci-dessous).
- **Failles du template à corriger avant mise en service** (à approuver, ne pas corriger en silence) :
  - `HandleExceptionMiddleware` expose l'exception sérialisée (§3).
  - CORS : `SetIsOriginAllowed(_ => true)` combiné à `AllowCredentials()` est dangereux ; restreindre aux origines du SPA.
- **Licences et conditions d'usage** des données Ametis et d'Amiens Métropole (P0.1). Conditionnent l'affichage, le cache et la réutilisation dans l'application publique.
- **Disponibilité réelle** des flux GTFS-RT : sans P0.3, les lots P3 et P5 ne peuvent pas être dimensionnés.
- **Hébergement** : conteneurs Docker sur Proxmox, mais systèmes d'exploitation, orchestration et stratégie de sauvegarde non précisés. Le template cible Azure DevOps : confirmer la plateforme CI.
- **Secrets et configuration** : gestionnaire retenu pour les clés MapTiler, OTP, SMTP, clés VAPID et chaîne de connexion. Aucun secret dans le dépôt, ni dans `appsettings.json`.
- **Positions GPS des utilisateurs (P8.1)** : la spec parle de « positions GPS avec horodatage » sans préciser s'il s'agit des véhicules (données publiques) ou des utilisateurs (données personnelles). Le régime RGPD et la base légale diffèrent.
- **Conflit de rétention** : « persister les positions historiques » (P3.3) n'indique pas de durée, alors que P8.1 fixe 30 jours. Confirmer la durée pour les positions véhicules.
- **Convention TypeScript** : la convention C# est donnée par `.editorconfig` du template ; aucune convention n'est définie pour le frontend.
- **Suivi des erreurs et de disponibilité (P9)** : outil non choisi.
- **Tests frontend et E2E** : outils non choisis.
- **Routage OTP** : le template ne contient aucun code OTP ; abstraction à concevoir au lot P6.
