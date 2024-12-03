///////////////////////////////
/////CREATION DU COMBOBOX//////
///////////////////////////////

// Example: Populate ComboBox with TIA versions
comboBox1.Items.Add("TIA V16");
comboBox1.Items.Add("TIA V17");
comboBox1.Items.Add("TIA V18");
comboBox1.SelectedIndex = 0;  // Default selection

////////////////////////////////////////////////
/////CHARGERZ LES REFERENCES DYNAMIQUEMENT//////
////////////////////////////////////////////////

using System.Reflection;

private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
{
    string selectedVersion = comboBox1.SelectedItem.ToString();
    
    // Clear current references (if necessary)

    // Load the appropriate DLL files based on the selected version
    string referencePath = "";
    if (selectedVersion == "TIA V16")
    {
        referencePath = @"References\TIA_Version1\";
    }
    else if (selectedVersion == "TIA V17")
    {
        referencePath = @"References\TIA_Version2\";
    }
    else if (selectedVersion == "TIA V18")
    {
        referencePath = @"References\TIA_Version3\";
    }

    // Dynamically load the assemblies (DLLs)
    LoadReferences(referencePath);
}

private void LoadReferences(string path)
{
    // Example: Load DLL dynamically
    Assembly.LoadFrom($"{path}SomeTIAReference.dll");
}	
/////////////////////////////////////////////////////
/////ENCAPSULEZ LE CHARGEMENT DES FONCTIONS TIA//////
/////////////////////////////////////////////////////

private void LoadTIAFunctions(string version)
{
    // Clear the current GroupBox content
    groupBoxTIAFunctions.Controls.Clear();

    // Based on the version, load the specific functions
    if (version == "TIA V16")
    {
        DisplayFunctionsForTIA_V16();
    }
    else if (version == "TIA V17")
    {
        DisplayFunctionsForTIA_V17();
    }
    else if (version == "TIA V18")
    {
        DisplayFunctionsForTIA_V18();
    }
}

private void DisplayFunctionsForTIA_V16()
{
    // Example of adding controls for TIA V16 functions
    Label label1 = new Label() { Text = "Function 1 for TIA V16", Location = new Point(10, 20) };
    groupBoxTIAFunctions.Controls.Add(label1);

    // Add more controls as needed
}

private void DisplayFunctionsForTIA_V17()
{
    // Example of adding controls for TIA V17 functions
    Label label2 = new Label() { Text = "Function 1 for TIA V17", Location = new Point(10, 20) };
    groupBoxTIAFunctions.Controls.Add(label2);

    // Add more controls as needed
}

private void DisplayFunctionsForTIA_V18()
{
    // Example of adding controls for TIA V18 functions
    Label label3 = new Label() { Text = "Function 1 for TIA V18", Location = new Point(10, 20) };
    groupBoxTIAFunctions.Controls.Add(label3);

    // Add more controls as needed
}


///////////////////////////////////////////////////////////////
/////APPELER LA METHODE LORS DU CHARGEMENT DE LA FONCTION//////
///////////////////////////////////////////////////////////////

private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
{
    string selectedVersion = comboBox1.SelectedItem.ToString();
    
    // Recharger uniquement la partie du GroupBox
    LoadTIAFunctions(selectedVersion);
}

///////////////////////////////////////////////
/////OPTIMISER LE RAFRAICHISSEMENT VISUEL//////
///////////////////////////////////////////////

private void LoadTIAFunctions(string version)
{
    // Clear the current GroupBox content
    groupBoxTIAFunctions.Controls.Clear();
    groupBoxTIAFunctions.Refresh();  // Force immediate redraw

    // Load the functions for the selected TIA version
    if (version == "TIA V16")
    {
        DisplayFunctionsForTIA_V16();
    }
    else if (version == "TIA V17")
    {
        DisplayFunctionsForTIA_V17();
    }
    else if (version == "TIA V18")
    {
        DisplayFunctionsForTIA_V18();
    }
}











L'erreur `InvalidCastException` se produit parce que le type des éléments dans `CbFunction.Items` est `String`, et non `UserControl`. Dans votre cas, `CbFunction.Items` contient des chaînes de texte représentant les noms des fonctions et non des instances de `UserControl`. Pour résoudre cela, voici deux options :

### Solution 1 : Utiliser un dictionnaire pour lier les noms des fonctions aux contrôles `UserControl`

Créez un dictionnaire pour associer chaque nom de fonction (chaîne) à une instance de `UserControl`. Cela permet de récupérer facilement le `UserControl` correspondant en utilisant le texte sélectionné dans le `ComboBox`.

1. **Déclarez un dictionnaire pour stocker les `UserControl`** :

    ```csharp
    private Dictionary<string, UserControl> functionControls = new Dictionary<string, UserControl>();
    ```

2. **Initialisez le dictionnaire** avec les paires nom-fonction / contrôle dans votre code d'initialisation (comme dans le constructeur ou une méthode de configuration).

    ```csharp
    functionControls.Add("Fonction1", new UserControl1());
    functionControls.Add("Fonction2", new UserControl2());
    // Ajoutez d'autres UserControls ici...
    ```

3. **Ajoutez les noms de fonction au `ComboBox`**. Vous ajoutez uniquement les noms de fonction dans `CbFunction` :

    ```csharp
    CbFunction.Items.Add("Fonction1");
    CbFunction.Items.Add("Fonction2");
    ```

4. **Récupérez le `UserControl` sélectionné à partir du dictionnaire** lorsque l’utilisateur sélectionne une fonction :

    ```csharp
    private void CbFunction_SelectedIndexChanged(object sender, EventArgs e)
    {
        GbFunctions.Controls.Clear();

        string selectedFunction = CbFunction.Text;

        // Vérifiez si le dictionnaire contient la clé
        if (functionControls.TryGetValue(selectedFunction, out UserControl selectedControl))
        {
            GbFunctions.Controls.Add(selectedControl);
            selectedControl.Dock = DockStyle.Fill;
        }
        else
        {
            MessageBox.Show("Contrôle non trouvé pour la fonction sélectionnée.");
        }
    }
    ```

### Solution 2 : Utiliser `Tag` pour lier chaque élément à un `UserControl`

Si vous préférez ne pas utiliser un dictionnaire, vous pouvez stocker l'instance `UserControl` dans la propriété `Tag` de chaque élément du `ComboBox`.

1. **Ajoutez des éléments dans le `ComboBox` avec la propriété `Tag`** définie pour chaque contrôle :

    ```csharp
    CbFunction.Items.Add(new ComboBoxItem("Fonction1") { Tag = new UserControl1() });
    CbFunction.Items.Add(new ComboBoxItem("Fonction2") { Tag = new UserControl2() });
    ```

2. **Récupérez le contrôle associé via `Tag` lors de la sélection** :

    ```csharp
    private void CbFunction_SelectedIndexChanged(object sender, EventArgs e)
    {
        GbFunctions.Controls.Clear();

        if (CbFunction.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is UserControl selectedControl)
        {
            GbFunctions.Controls.Add(selectedControl);
            selectedControl.Dock = DockStyle.Fill;
        }
    }
    ```

Ces méthodes devraient résoudre votre erreur en vous permettant d'accéder directement aux instances `UserControl` associées à chaque option dans le `ComboBox`.
