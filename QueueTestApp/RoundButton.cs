using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class RoundButton : Button
{
    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
    {
        GraphicsPath grPath = new GraphicsPath();
        grPath.AddEllipse(3, 3, ClientSize.Width-6, ClientSize.Height-6);
        this.Region = new System.Drawing.Region(grPath);
        base.OnPaint(e);
    }
}
