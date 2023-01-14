using MagicUI.Core;
using MagicUI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ButtplugKnight
{
    public static class VibeUI
    {
        public static TextObject textUI = null;
        public static void Setup(LayoutRoot layout)
        {
            textUI = new TextObject(layout)
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                TextAlignment = HorizontalAlignment.Right,
                FontSize = 40,
                Font = UI.TrajanBold,
                Text = ""
            };
        }
    }
}
