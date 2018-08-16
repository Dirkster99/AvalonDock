namespace Settings.UserProfile
{
    using System.Windows;

    /// <summary>
    /// Provide an interface to implement saving and loading/repositioning of Window or view.
    /// </summary>
    public interface IViewSize
    {
        //
        // Zusammenfassung:
        //     Ruft die Position des linken Fensterrands im Verhältnis zum Desktop ab oder legt
        //     diese fest.
        //
        // Rückgabewerte:
        //     Die Position des linken Fensterrands in logischen Einheiten (1/96 Zoll).
        double Left { get; set; }

        //
        // Zusammenfassung:
        //     Ruft die Position des oberen Fensterrands im Verhältnis zum Desktop ab oder legt
        //     diese fest.
        //
        // Rückgabewerte:
        //     Die Position des oberen Fensterrands in logischen Einheiten (1/96 ").
        double Top { get; set; }

        //
        // Zusammenfassung:
        //     Ruft die Breite des Elements ab bzw. legt diese fest.
        //
        // Rückgabewerte:
        //     Die Breite des Elements in geräteunabhängige Einheiten (1/96th inch per unit).Der
        //     Standardwert ist System.Double.NaN.Dieser Wert muss größer oder gleich 0,0 sein.In
        //     den Hinweisen finden Sie Informationen über obere Grenzen.
        double Width { get; set; }

        //
        // Zusammenfassung:
        //     Ruft die vorgeschlagene Höhe des Elements ab oder legt diese fest.
        //
        // Rückgabewerte:
        //     Die Höhe des Elements in geräteunabhängige Einheiten (1/96th inch per unit).Der
        //     Standardwert ist System.Double.NaN.Dieser Wert muss größer oder gleich 0,0 sein.In
        //     den Hinweisen finden Sie Informationen über obere Grenzen.
        double Height { get; set; }

        //
        // Zusammenfassung:
        //     Ruft einen Wert ab, der angibt, ob ein Fenster wiederhergestellt, minimiert oder
        //     maximiert ist, oder legt diesen fest.
        //
        // Rückgabewerte:
        //     Ein System.Windows.WindowState, der bestimmt, ob ein Fenster wiederhergestellt,
        //     minimiert oder maximiert ist.Der Standardwert ist System.Windows.WindowState.Normal
        //     (wiederhergestellt).
        WindowState WindowState { get; set; }
    }
}
