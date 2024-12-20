using FSO.Client.UI.Controls;
using FSO.Client.UI.Framework;
using FSO.Common;
using FSO.Content;
using System.Collections.Generic;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using FSO.Content.Model;
using FSO.Client.UI.Panels.Chat;

namespace FSO.Client.UI.Panels
{
    public class UIAssetMenu : UIDialog
    {
        public UIListBox BookmarkListBox { get; set; }
        public UIListBoxTextStyle BookmarkListBoxColors { get; set; }

        public UISlider BookmarkListSlider { get; set; }
        public UIButton BookmarkListScrollUpButton { get; set; }
        public UIButton BookmarkScrollDownButton { get; set; }
        public UIButton SimsTabButton { get; set; }
        public UIButton IgnoreTabButton { get; set; }

        
        public UITextBox CacheSearch { get; set; }


        public UIChatCategoryButton ButtonDumpTextures { get; set; }
        public UIChatCategoryButton ButtonClearCache { get; set; }


        public UIAssetMenu() : base(UIDialogStyle.Standard, true)
        {
            // Objects
            var ui = this.RenderScript("bookmarks.uis");
            Caption = "Cache Debug Menu";
            Remove(CloseButton);
            Remove(SimsTabButton);
            Remove(IgnoreTabButton);
            Remove(base.CloseButton);

            // Object logger
            var listBg = ui.Create<UIImage>("ListBoxBackground");
            AddAt(4, listBg);
            listBg.With9Slice(25, 25, 25, 25);
            listBg.Width += 50;
            listBg.Height += 180;

            BookmarkListBox.SetSize(BookmarkListBox.Size.X + 50, BookmarkListBox.Size.Y);
            BookmarkListBox.VisibleRows += 10;
            BookmarkListScrollUpButton.X += 50;
            BookmarkListSlider.SetSize(10, 170 + 180);
            BookmarkScrollDownButton.X += 50;
            BookmarkScrollDownButton.Y += 180;
            BookmarkListSlider.X += 50;
            BookmarkListSlider.AttachButtons(BookmarkListScrollUpButton, BookmarkScrollDownButton, 1);
            BookmarkListBox.AttachSlider(BookmarkListSlider);
            BookmarkListBoxColors = ui.Create<UIListBoxTextStyle>("BookmarkListBoxColors", BookmarkListBox.FontStyle);

            // Object search
            CacheSearch = new UITextBox();
            CacheSearch.Size = new(300, 25);
            CacheSearch.X = 16;
            CacheSearch.Y = 500 - 50;
            CacheSearch.OnEnterPress += new KeyPressDelegate(CacheSearch_OnEnterPress);
            Add(CacheSearch);

            // Pools
            var listBg2 = ui.Create<UIImage>("ListBoxBackground");
            AddAt(4, listBg2);
            listBg2.With9Slice(25, 25, 25, 25);
            listBg2.X += 322;
            listBg2.Width = 453 - 267 - 15;
            listBg2.Height += 80;

            // Actions
            ButtonDumpTextures = new UIChatCategoryButton
            {
                Position = new(26, 24),
                Caption = "Dump Textures"
            };
            ButtonDumpTextures.OnButtonClick += new ButtonClickDelegate(ButtonDumpTextures_OnButtonPress);
            Add(ButtonDumpTextures);

            UpdateLists("");

            SetSize(550, 500);
        }

        public void UpdateLists(string match)
        {
            List<UIListBoxItem> list = new List<UIListBoxItem>();

            foreach (var pool in CacheControler.Pools)
            {
                foreach (var obj in pool.GetContents())
                {
                    var line = obj.Value + " " + obj.Key;
                    if (line.Contains(match) || match == "")
                        list.Add(new UIListBoxItem("", line));
                }
            }

            BookmarkListBox.Columns[0].Alignment = TextAlignment.Left | TextAlignment.Top;
            BookmarkListBox.Columns[0].Width = (int)BookmarkListBox.Width;
            BookmarkListBox.Items = list;
        }

        public void DumpTextures()
        {
            var dumped = 0;
            Directory.CreateDirectory(Path.Combine(FSOEnvironment.ContentDir, "Dump"));
            for (var i = 0; CacheControler.Pools.Count > i; i++)
                foreach (var obj in CacheControler.Pools[i].GetContents())
                    if (CacheControler.Cached(i, obj.Key) && typeof(ITextureRef).IsAssignableFrom(CacheControler.GetType(i, obj.Key)))
                    {
                        var tex = CacheControler.Get<ITextureRef>(i, obj.Key).GetImage();
                        byte[] buffer = new byte[tex.Width * tex.Height * 4];
                        if (tex.PixelSize == 4)
                            using (var image = Image.LoadPixelData<Bgra32>(tex.Data, tex.Width, tex.Height))
                                image.SaveAsPng(Path.Combine(FSOEnvironment.ContentDir, "Dump", (obj.Value == "" ? obj.Key : obj.Value) + ".png"));
                        else if (tex.PixelSize == 3)
                            using (var image = Image.LoadPixelData<Bgr24>(tex.Data, tex.Width, tex.Height))
                                image.SaveAsPng(Path.Combine(FSOEnvironment.ContentDir, "Dump", (obj.Value == "" ? obj.Key : obj.Value) + ".png"));
                        else
                            throw new System.Exception("Unexpected bpp for image " + obj.Key);
                        using (var image = Image.LoadPixelData<Bgra32>(tex.Data, tex.Width, tex.Height))
                            image.SaveAsPng(Path.Combine(FSOEnvironment.ContentDir, "Dump", (obj.Value == "" ? obj.Key : obj.Value) + ".png"));
                        dumped++;
                    }

            UIAlert savedDialog = null;
            savedDialog = new UIAlert(new UIAlertOptions
            {
                Title = "Cached Textures Exported!",
                Message = "Dumped " + dumped.ToString() + " textures, you can find them in Content/Dump!",
                TextEntry = false,
                Width = 360,
                Buttons = new UIAlertButton[]
                {
                    new UIAlertButton(UIAlertButtonType.OK, (b) =>
                    {
                        UIScreen.RemoveDialog(savedDialog);
                        savedDialog = null;
                    })
                }
            });
            UIScreen.GlobalShowDialog(savedDialog, true);
        }




        private void CacheSearch_OnEnterPress(UIElement search)
        {
            UpdateLists(((UITextBox)search).CurrentText);
        }

        private void ButtonDumpTextures_OnButtonPress(UIElement button)
        {
            DumpTextures();
        }
    }

    internal struct UIAssetEntry
    {
        public string Label { get; set; }
        public ulong Id { get; set; }
    };
}
