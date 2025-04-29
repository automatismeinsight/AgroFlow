# Interface Principale – AgroFlow

## Présentation

L’**interface principale** d’AgroFlow est le point d’entrée graphique de l’application, pensée pour offrir une expérience fluide et efficace aux automaticiens et chefs de projet du groupe Agro Mousquetaires.  
Elle centralise l’accès aux fonctions critiques, facilite la gestion multi-utilisateur (avec distinction admin/utilisateur standard) et permet une navigation rapide entre les différents modules métiers de l’outil.

## Fonctionnement global

- **Interface graphique Windows Forms** moderne et personnalisée, adaptée au standard Agro Mousquetaires.
- **Sélection dynamique de la version TIA Portal**?: l’utilisateur choisit la version TIA installée avec mémorisation du dernier choix.
- **Navigation modulaire** : chaque fonction (aide au diagnostic, réception de projet, etc.) s’ouvre sous forme de UserControl pour garantir rapidité et clarté.
- **Gestion centralisée des logs**?: toutes les actions importantes sont historisées dans un module de log accessible à tout moment.
- **Menu principal rétractable/étirable** pour optimiser l’espace de travail ou accéder à des fonctions avancées (admin, terminal, etc.).
- **Gestion avancée de la fenêtre**?: redimensionnement, maximisation, déplacement personnalisé, etc.

## Principales fonctionnalités accessibles depuis l’interface

- **Aide au diagnostic**?: génération automatisée des variables BLKJump dans le FC d’aide au diagnostic (voir documentation fonction dédiée).
- **Réception de projet**?: intégration facilitée et vérification de conformité de nouveaux projets TIA Portal
- **Historique des actions/logs**?: affichage instantané de toutes les opérations réalisées via l’interface.

## Public visé

- **Automaticiens**, chefs de projet, intégrateurs travaillant sur des projets TIA Portal Agro Mousquetaires.
- **Administrateurs**?: accès à des outils supplémentaires pour la maintenance et la configuration avancée.

## Prérequis techniques

- Application **AgroFlow** installée sur le poste de travail.
- Une ou plusieurs versions de **TIA Portal** (V17 minimum) installées sur le poste.
- Références internes Agro Mousquetaires (bibliothèque LTU, standards de projet…).

## Parcours utilisateur type

1. **Sélection de la version TIA Portal** installée (la sélection est mémorisée entre les sessions).
2. **Choix d’une fonction** selon le profil utilisateur (aide au diagnostic, réception de projet…).
4. **Utilisation des modules**?: chaque fonction s’ouvre dans l’interface principale, avec gestion des logs et navigation simplifiée.
5. **Gestion ergonomique de la fenêtre** (maximisation, réduction, déplacement, etc.).

## Extension et personnalisation

L’interface principale est conçue pour être **évolutive**?:  
- De nouveaux modules (UserControls) peuvent être ajoutés facilement.
- Les droits et profils utilisateurs sont centralisés pour une gestion simplifiée.
- La personnalisation des couleurs, logos, et messages respecte la charte Agro Mousquetaires.

---

*Ce module d’interface graphique AgroFlow incarne la volonté du groupe Agro Mousquetaires de proposer des outils performants, ergonomiques et adaptés aux besoins des équipes d’automatisme industrielle.*
