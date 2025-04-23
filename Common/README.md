# Gestion dynamique des versions TIA Portal – AgroFlow

## Présentation

La gestion des versions TIA Portal dans AgroFlow est une **fonctionnalité centrale et très pratique** permettant de sélectionner dynamiquement la version TIA Portal à utiliser, sans devoir installer ou lancer une application différente pour chaque version.  
**Avant cette innovation, il existait une application distincte pour chaque version de TIA Portal** (V16, V17, etc.), ce qui compliquait la maintenance, la distribution et l’utilisation sur le terrain.  
Grâce à la gestion dynamique, un seul outil AgroFlow suffit désormais pour piloter l’ensemble des versions supportées de TIA Portal Openness?: un gain de temps, de praticité et de robustesse pour tous les automaticiens et chefs de projet.

## Fonctionnement global

- **Sélection de la version TIA Portal**?: à l’ouverture ou depuis l’interface principale, l’utilisateur choisit la version installée sur son poste.
- **Vérification automatique** de la disponibilité des DLL Openness nécessaires (Siemens.Engineering.dll, Siemens.Engineering.Hmi.dll...).
- **Chargement dynamique** des assemblies Openness via le gestionnaire d’assemblies .NET, sans redémarrage ni manipulation complexe.
- **Gestion des changements de version**?: lors d’un changement, les modules en cours sont déchargés proprement, évitant tout conflit de version ou plantage.
- **Interopérabilité transparente**?: les outils, scripts et modules internes utilisent toujours la bonne version TIA Portal, adaptée au projet en cours.

## Classes et modules principaux

- **TIAVersionManager**?:  
  Permet de définir, mémoriser, et surveiller la version courante de TIA Portal utilisée.  
  Fournit également des méthodes pour vérifier la présence des DLL nécessaires, et signale tout changement de version à l’application.

- **TIAAssemblyLoader**?:  
  S’occupe du chargement dynamique des DLL Openness appropriées et de la gestion des contrôles utilisateurs (.NET/WinForms) dépendants de la version courante.
  Gère également le déchargement propre des modules lors d’un changement de version.

## Avantages pour l’utilisateur

- **Un seul outil pour toutes les versions**?: fini les applications multiples, tout est centralisé.
- **Changement de version ultra simple**?: un simple choix dans l’interface, sans redémarrage complexe.
- **Sécurité et robustesse**?: impossible de charger une mauvaise DLL ou de provoquer des conflits de version.
- **Gain de temps majeur** pour la maintenance, la formation et le support?: un seul logiciel à installer et à maîtriser.

## Prérequis

- TIA Portal installé (V17 minimum)
- Les DLL Openness Siemens correspondantes présentes sur le poste
- Application AgroFlow (version récente)

## Pour aller plus loin

La documentation XML détaillée (dans le code source) décrit chaque méthode, propriété et événement des modules `TIAVersionManager` et `TIAAssemblyLoader`.

---

*Cette gestion dynamique des versions TIA Portal est un atout majeur d’AgroFlow, pensée pour la productivité et la simplicité d’usage sur tous les sites industriels Agro Mousquetaires.*