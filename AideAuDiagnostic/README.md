# AgroFlow - Fonction d'Aide au Diagnostic TIA Portal

## Présentation

Cette fonction fait partie de l’application **AgroFlow**, développée en interne chez AgroMousquetaires, et vise à faciliter le travail des automaticiens.  
Elle est spécifiquement destinée aux projets TIA Portal AgroMousquetaires intégrant l’aide au diagnostic.

L’outil parcourt automatiquement le projet TIA Portal afin de générer les variables **BLKJump** dans le FC "Aide au Diagnostic".  
Il s’agit d’un processus automatisé et fiable pour la préparation des automatismes, en conformité avec les standards AgroMousquetaires.

## Architecture & Classes principales

Le cœur de cette fonctionnalité repose sur trois classes principales :

- **ExploreTIA** : Parcours l’ensemble du projet TIA Portal à la recherche des blocs et informations nécessaires à l’aide au diagnostic.
- **PlcGenerateTiaCode** : Génère le code TIA Portal approprié (variables, blocs, etc.), notamment les instructions BLKJump dans le FC dédié.
- **HMATIAOpenness** : Gère la connexion, l’ouverture/sauvegarde du projet TIA Portal, ainsi que les opérations Openness (API Siemens).

## Flux global & fonctionnement

1. **Connexion** au projet TIA Portal AgroMousquetaires (via HMATIAOpenness)
2. **Analyse** du projet et de la bibliothèque LTU (ExploreTIA)
3. **Génération** automatique des structures de diagnostic (PlcGenerateTiaCode)
4. **Intégration** des variables BLKJump dans le FC "Aide au Diagnostic"
5. **Sauvegarde** et validation du projet

## Prérequis

- **TIA Portal** version 17 minimum
- **Application AgroFlow** installée sur le poste de développement
- **Projet TIA Portal** conforme AgroMousquetaires, incluant la bibliothèque **LTU**

## Public visé

- Automaticiens et intégrateurs travaillant sur les projets TIA AgroMousquetaires
- Développeurs chargés de la maintenance ou de l’évolution des outils AgroFlow

## Scénarios d’utilisation

- Préparation d’un projet TIA Portal AgroMousquetaires pour exploitation avec l’aide au diagnostic
- Génération ou mise à jour des variables BLKJump dans le FC "Aide au Diagnostic"

## Pour aller plus loin

La documentation XML générée (voir les commentaires `<summary>` dans le code source) détaille chaque classe, méthode et propriété du projet.  
Ce document sert de référence technique pour toute maintenance ou évolution future.

---

*Cette fonctionnalité AgroFlow est un outil interne AgroMousquetaires, optimisé pour la productivité et la qualité du support automatisme sur les lignes de production agroalimentaires.*