# AgroFlow – Application Globale d’Automatisation TIA Portal

## 1 Contexte

Le projet **AgroFlow** a été conçu pour optimiser les processus de travail des chefs de projet et des automaticiens du groupe **Agro Mousquetaires**. Il s’appuie sur l’API Openness de TIA Portal (Siemens) afin de répondre aux besoins croissants d’intégration entre les systèmes IT (Information Technology) et OT (Operational Technology). L’ambition est d’améliorer l’efficacité, la traçabilité et l’automatisation des tâches dans le domaine industriel, tout en favorisant la digitalisation des outils métiers.

## 2 Objectifs

### Intégration IT/OT

- **Faciliter l’interconnexion** entre les environnements IT et OT pour une gestion industrielle unifiée.
- **Réduire les silos** entre équipes et systèmes pour fluidifier la circulation des données.
- **Favoriser la collaboration** et le partage d’informations entre les différents niveaux de l’entreprise.

### Maîtrise de TIA Portal Openness

- **Apprendre et exploiter l’API Openness** pour automatiser les opérations récurrentes dans TIA Portal.
- **Optimiser les tâches complexes** et minimiser les interventions manuelles grâce à des scripts robustes.

### Développement de scripts/outils personnalisés

- **Concevoir des outils sur mesure** pour automatiser les workflows, l’import/export de données, ou la gestion de configuration.
- **Simplifier les échanges de données** entre supervision (niveau N2), gestion des données (N3), et automatisme.

## 3 Besoins Fonctionnels

### Facilitation du travail des chefs de projet et automaticiens

- **Gestion centralisée et simplifiée** des projets TIA Portal (création, paramétrage, archivage).
- **Automatisation** de l’import/export de données, de la configuration des projets et du déploiement des mises à jour.
- **Réception et intégration rapide de nouveaux projets**, avec vérification de conformité (bibliothèque LTU, standards internes).

### Interface utilisateur intuitive

- **Interface graphique claire et accessible**, adaptée aussi bien aux automaticiens qu’aux utilisateurs non techniques.
- **Navigation rapide** entre projets, gestion des droits utilisateurs, et accès aux principales fonctionnalités en quelques clics.

### Performances et robustesse

- **Exécution rapide** des scripts, gestion efficace de gros volumes de données ou de projets complexes.
- **Stabilité et fiabilité** dans l’automatisation des processus industriels.

## 4 Technologie et outils

| Technologie/Outil         | Version       | Rôle dans le projet                                      |
|--------------------------|--------------|----------------------------------------------------------|
| **C#**                   | .NET 7.0     | Développement principal de l’application                 |
| **TIA Portal Openness**  | 17, 18, 19…  | API Siemens pour automatiser et interagir avec TIA Portal|
| **Visual Studio**        | 2022         | IDE principal pour le développement C#                   |
| **Git**                  | GitHub/GitLab| Versionnement, gestion de code et collaboration          |
| **Windows Forms**        | .NET 4.8     | Interface utilisateur graphique                          |

> **Nota :** L’application s’inscrit dans une démarche évolutive et modulaire, avec des mises à jour régulières pour suivre les évolutions de TIA Portal et des besoins internes.

## 5 Fonctionnalités principales

L’application AgroFlow est structurée autour de **plusieurs modules/fonctions** majeurs, dont notamment :

- **Aide au diagnostic** :  
  Génération automatisée des variables BLKJump dans le FC "Aide au Diagnostic", permettant un diagnostic rapide et fiable des automates, conformément aux standards Agro Mousquetaires.

- **Réception de projet** :  
  Fonction d’intégration rapide d’un nouveau projet TIA Portal, avec vérification de la conformité (bibliothèque LTU, structures standards, etc.) et automatisation de la configuration initiale.

- **(À compléter selon modules additionnels : export/import de données, génération de rapports, gestion des utilisateurs, etc.)**

Chaque module est pensé pour réduire le temps passé sur les tâches administratives ou répétitives, et offrir un maximum de contrôle et de visibilité sur la configuration des automates.

---

*AgroFlow est un outil interne du groupe Agro Mousquetaires, dédié à l’industrialisation, la qualité et la performance des projets d’automatisme et de digitalisation industrielle.*
