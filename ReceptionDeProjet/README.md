# AgroFlow - Fonction de Réception de Projet

## Présentation

Cette fonction fait partie de l’application **AgroFlow**, développée en interne chez AgroMousquetaires, et vise à faciliter la gestion et l’intégration des projets TIA Portal lors de leur réception.  
Elle est spécifiquement conçue pour faciliter et assurrer la reception de projet, en conformité avec les exigences AgroMousquetaires.

L’outil permet d’automatiser les vérifications, l’import et l’intégration des projets TIA Portal reçus, garantissant ainsi la conformité aux standards et facilitant la prise en main par les automaticiens.

## Architecture & Classes principales

Le cœur de cette fonctionnalité repose sur plusieurs classes clés :

- **ReceptionTIA** : Gère l’import du projet TIA Portal, réalise les contrôles de conformité et prépare le projet pour l’exploitation interne.
- **ProjectValidator** : Effectue une série de vérifications automatiques pour s’assurer que le projet respecte les standards AgroMousquetaires (structure, nommage, bibliothèques, etc.).
- **HMATIAOpenness** : Assure la connexion, l’ouverture et la sauvegarde des projets TIA Portal via l’API Openness Siemens.

## Flux global & fonctionnement

1. **Import** du projet TIA Portal reçu (via ReceptionTIA)
2. **Gestion interne** et création d'objet pour les devices, blocs...
3. **Contrôle automatisé** de la conformité du projet (ProjectValidator)
4. **Intégration** dans l’environnement AgroFlow pour exploitation
5. **Sauvegarde** et validation finale du projet

## Prérequis

- **TIA Portal** version 17 minimum (V19 conseillé, il est possible que les versions précédente ommetes certains paramètres)
- **Application AgroFlow** installée sur le poste de développement
- **Projet TIA Portal** ouvert sur le bureau

## Public visé

- Automaticiens, intégrateurs et chefs de projet en charge de la réception, de l’intégration et du développement des projets TIA AgroMousquetaires
- Développeurs assurant la maintenance ou l’évolution de l’outil AgroFlow

## Scénarios d’utilisation

- Réception et contrôle de conformité d’un nouveau projet TIA Portal AgroMousquetaires
- Synthèse simplifier et rapide accèssible à tous

## Pour aller plus loin

La documentation XML générée (voir les commentaires `<summary>` dans le code source) détaille chaque classe, méthode et propriété du projet.  
Ce document constitue une référence technique pour la maintenance et l’évolution future de cette fonctionnalité.

---

*Cette fonctionnalité AgroFlow est un outil interne AgroMousquetaires, conçu pour fiabiliser et simplifier le processus de réception des projets automatisme dans l’industrie agroalimentaire.*
