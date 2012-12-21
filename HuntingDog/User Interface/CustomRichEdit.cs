/*public class RichTextLabel : RichTextBox
{
    public RichTextLabel()
    {
        base.ReadOnly = true;
        base.BorderStyle = BorderStyle.None;
        base.TabStop = false;
        base.SetStyle(ControlStyles.Selectable, false);
        base.SetStyle(ControlStyles.UserMouse, true);
        base.SetStyle(ControlStyles.SupportsTransparentBackColor, true);

        base.MouseEnter += delegate(object sender, EventArgs e)
        {
            this.Cursor = Cursors.Default;
        };
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 0x204) return; // WM_RBUTTONDOWN
        if (m.Msg == 0x205) return; // WM_RBUTTONUP
        base.WndProc(ref m);
    }
}*/