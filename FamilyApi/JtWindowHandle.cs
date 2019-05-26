using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace FamilyApi
{
    public class JtWindowHandle : IWin32Window
  {
    IntPtr _hwnd;

    public JtWindowHandle( IntPtr h )
    {
      Debug.Assert( IntPtr.Zero != h,
        "expected non-null window handle" );

      _hwnd = h;
    }

    public IntPtr Handle
    {
      get
      {
        return _hwnd;
      }
    }
  }
}
