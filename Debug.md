# Debugging Guide

## Problème avec Tuple

### Symptômes
Si votre projet rencontre un problème lié à `Tuple`, cela peut être dû à une dépendance non nécessaire ou mal configurée.

### Solution
1. **Ne pas installer la dépendance** : Assurez-vous de ne pas ajouter explicitement la dépendance `Tuple` dans votre projet.
2. **Rechercher `Tuple` dans `App.config`** : Utilisez `Ctrl+F` pour rechercher la mention de `Tuple`.
3. **Supprimer l'entrée `dependentAssembly`** : Localisez et supprimez l'entrée correspondante dans le fichier `App.config`.

Exemple d'entrée à supprimer :
```xml
<dependentAssembly>
  <assemblyIdentity name="System.ValueTuple" publicKeyToken="cc7b13ffcd2ddd51" culture="neutral" />
  <bindingRedirect oldVersion="0.0.0.0-4.0.3.0" newVersion="4.0.3.0" />
</dependentAssembly>
```
### Vérification
* Compilez votre projet pour vérifier que l'erreur a bien disparu.
* Si le problème persiste, vérifiez que Tuple n'est pas importé par une autre dépendance.
