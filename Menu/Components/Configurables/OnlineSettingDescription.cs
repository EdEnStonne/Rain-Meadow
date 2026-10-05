using Menu;
using Menu.Remix;
using UnityEngine;
using static RainMeadow.UI.Components.OnlineSlugcatAbilitiesInterface;

namespace RainMeadow.UI.Components.Configurables;

public class OnlineSettingDescription : OnlineSettingElement
{
    public const string LINEBREAK = "<LINE>";
    public static readonly Color defaultColor = Color.Lerp(MenuColorEffect.rgbMediumGrey, MenuColorEffect.rgbDarkGrey, 0.5f);
    public const float textMargin = -7.5f;
    public override MenuObject selectable => descriptionLabel;
    public override int additionalPositionsTaken 
        => Mathf.Max(0, ((int)descriptionLabel.label.textRect.height / (int)size.y) - 1);
    public MenuLabel descriptionLabel;
    public Color color = defaultColor;
    public OnlineSettingDescription(Menu.Menu menu, OnlineSlugcatSettingsBase owner, string description, OnlineSettingTab? tab = null)
        : this(menu, owner.scroller, description, tab) {}
    public OnlineSettingDescription(Menu.Menu menu, MenuObject owner, string description, OnlineSettingTab? tab = null) : base(menu, owner, tab)
    {
        descriptionLabel = new(
            menu,
            this,
            menu.Translate(description).Replace(LINEBREAK, "\n"),
            Vector2.zero,
            size,
            false
        );
        descriptionLabel.label.alignment = FLabelAlignment.Left;

        // MenuLabel testLabel = new(
        //     menu,
        //     this,
        //     "---------------------------",
        //     Vector2.left * textSpacing / 2f,
        //     new(textSpacing, elementHeight),
        //     false
        // );
        // testLabel.label.alignment = FLabelAlignment.Left;

        this.SafeAddSubobjects(descriptionLabel);
    }

    public override void Update()
    {
        base.Update();
        
        descriptionLabel.pos = new Vector2(-size.x/2 + textMargin, -descriptionLabel.label.textRect.height/2 + descriptionLabel.label.FontLineHeight/2);
    }
    public override void GrafUpdate(float timeStacker)
    {
        base.GrafUpdate(timeStacker);
        descriptionLabel.label.color = color;
        descriptionLabel.label.alpha = currentAlpha * (grayedOut ? 0.75f : 1);
        descriptionLabel.label.isVisible = visible;
    }
}