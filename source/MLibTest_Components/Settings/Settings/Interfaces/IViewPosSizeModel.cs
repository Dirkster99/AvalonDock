namespace Settings.Interfaces
{
    using System;

    public interface IViewPosSizeModel
    {
        bool DefaultConstruct { get; }
        double Height { get; set; }
        bool IsMaximized { get; set; }
        double Width { get; set; }
        double X { get; set; }
        double Y { get; set; }

        void SetValidPos(double SystemParameters_VirtualScreenLeft, double SystemParameters_VirtualScreenTop);
    }
}
