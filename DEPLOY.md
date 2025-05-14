# Procédure de Déploiement d’une Mise à Jour – Projet AgroFlow

Ce guide détaille les étapes à suivre pour déployer une nouvelle version du projet après validation et push du code.

---

## 1. Mise à jour des versions

- Ouvrir **Propriétés** du projet `MainInterface`  
  Chemin : `MainInterface` → `Propriétés` → `Application` → **Informations de l'assembly**
- Mettre à jour :
  - **Version d’assembly**
  - **Version du fichier**
- Respecter le format `1.x.x`, à incrémenter selon l’importance de la modification.

---

## 2. Mise à jour de la méthode Updater

- Fichier : `MainInterface/MainForms.cs`
- Méthode : `public void Updater()`
- Mettre à jour la version dans l’appel à `Contains("1.x.x")`.

**Exemple :**
```csharp
public void Updater()
{
    WebClient webClient = new WebClient();
    if (!webClient.DownloadString("https://raw.githubusercontent.com/SamBzd/AgroFlow/refs/heads/master/Update.txt").Contains("1.x.x"))
    {
        // ...
    }
}
```
Remplacer `1.x.x` par la nouvelle version.

---

## 3. Mise à jour du setup

- Ouvrir `Outils/AgroFlowSetup`
- Onglet **Propriété**
- Mettre à jour la **Version** : `1.x.x`

---

## 4. Mise à jour de la documentation

- Mettre à jour la version dans le titre de la documentation.
- **Si nouvelle fonction :**
  - Ajouter un commentaire XML à la méthode/fonction concernée.
  - Ajouter une page de présentation pour la nouvelle fonctionnalité.

---

## 5. Fichier de version GitHub

- Ouvrir le fichier : `GitHub/AgroFlow/Update.txt`
- Mettre à jour : écrire uniquement la nouvelle version (`1.x.x`)

---

## 6. Compilation

- Compiler chaque projet individuellement :
  - Faire la documentation en dernier.
  - Compiler le setup (`AgroFlowSetup`) en tout dernier.

---

## 7. Archivage

- Créer une archive `.zip` nommée :  
  **AgroFlowSetup.zip**

---

## 8. Déploiement sur GitHub

- Utiliser la fonction **Add File** sur GitHub pour uploader le fichier `.zip`
- ⚠️ Si le nom `AgroFlowSetup.zip` est respecté, l’ancienne version sera remplacée automatiquement (pas de doublons).

---

## Fin de la procédure

Veillez à bien suivre chacune de ces étapes pour assurer un déploiement propre et cohérent de la nouvelle version.
