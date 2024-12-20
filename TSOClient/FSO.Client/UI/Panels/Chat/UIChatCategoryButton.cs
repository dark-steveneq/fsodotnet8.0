using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;

namespace FSO.Client.UI.Panels.Chat
{
    public class UIChatCategoryButton : UIButton
    {
        private static TextStyle _Style = null;

        public UIChatCategoryButton() :
            base(Content.Content.Get().CustomUI.Get("chat_cat.png").Get(GameFacade.GraphicsDevice))
        {
            if (_Style == null)
            {
                _Style = TextStyle.DefaultLabel.Clone();
                _Style.Size = 8;
                _Style.Shadow = true;
            }
            CaptionStyle = _Style.Clone();
        }
    }
}
