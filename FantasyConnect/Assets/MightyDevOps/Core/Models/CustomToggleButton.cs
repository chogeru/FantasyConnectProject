using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Mighty.MightyCoreData;

namespace Mighty
{
    public class CustomToggleButton : VisualElement, INotifyValueChanged<bool>
    {
        //public event Action<bool> OnToggleStateChanged;


        private Texture2D onIcon;
        private Texture2D offIcon;
        public bool isToggledOn;
        private Label buttonName;
        private VisualElement optionsButton;
        public MappableTypeInfo mappableTypeInfo;

        public CustomToggleButton(Texture2D onIcon, MappableTypeInfo mappableTypeInfoRef)
        {
            OpenModuleSubMenu += OpenSubMenu;
            CloseModuleSubMenu += CloseSubMenu;

            mappableTypeInfo = mappableTypeInfoRef;

            this.onIcon = onIcon;
            offIcon = CreateGrayscaleIcon(onIcon);

            style.width = 32;
            style.height = 32;
            style.top = 0;
            style.left = 0;
            isToggledOn = true;
            style.backgroundImage = new StyleBackground(onIcon);  // Set initial icon

            style.transitionProperty = new List<StylePropertyName>
            {
                new StylePropertyName("left"),
                new StylePropertyName("top"),
                new StylePropertyName("width"),
                new StylePropertyName("height"),
            };

            style.transitionDuration = new List<TimeValue>()
            {
                new TimeValue(transitionSpeed, TimeUnit.Millisecond)
            };

            buttonName = new Label(mappableTypeInfo.Name)
            {
                style =
            {
                width = 80,
                position = Position.Absolute,
                backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.9f),
                color = Color.white,
                visibility = Visibility.Hidden
            }
            };
            this.Add(buttonName);

            DevLog("mappableRef.Name: " + mappableTypeInfoRef.Name);

            optionsButton = new VisualElement()
            {
                style =
            {
                width = 16,
                height = 16,
                position = Position.Absolute,
                backgroundImage = new StyleBackground(icons.blueGearIcon),
                visibility = Visibility.Hidden
            }
            };
            //this.Add(optionsButton);


            RegisterCallback<MouseEnterEvent>(evt => ShowElements());
            RegisterCallback<MouseLeaveEvent>(evt => HideElements());
            RegisterCallback<MouseDownEvent>(evt => Toggle());
            optionsButton.RegisterCallback<MouseDownEvent>((evt) =>
            {
                selectedModule = this;
                if (moduleSubMenuActive)
                {
                    CloseModuleSubMenu?.Invoke();
                    moduleSubMenuActive = false;
                }
                else
                {
                    OpenModuleSubMenu?.Invoke();
                    moduleSubMenuActive = true;
                }
                //OpenModuleSubMenu?.Invoke();

                evt.StopPropagation();
                // Alternatively, to slide 96px to the left
                // float targetLeft = style.left.value.value - 96;
                // style.left = new StyleLength(targetLeft);
            });


        }

        public float topCache = 0f;
        private void OpenSubMenu()
        {
            // if (selectedModule == this)
            // {
            //     // Assuming the parent container's top left corner is at (0, 0)
            //     // float targetTop = -this.worldBound.y + this.parent.worldBound.y;
            //     style.top = 0;//new StyleLength(targetTop);
            //     style.width = 48;
            //     style.height = 48;
            // }
            // else
            // {
            // Assuming the parent container's top left corner is at (0, 0)
            style.left = -96;
            style.top = topCache;
            // }
        }


        private void CloseSubMenu()
        {
            style.top = topCache;
            style.left = 0;
            // if (selectedModule == this)
            // {
            //     style.top = topCache;
            //     style.width = 96;
            //     style.height = 96;
            // }
            // else
            // {
            //     // Assuming the parent container's top left corner is at (0, 0)
            //     style.left = 0;
            // }
        }

        private void ShowElements()
        {
            buttonName.style.visibility = Visibility.Visible;
            optionsButton.style.visibility = Visibility.Visible;

            // Position the tooltip
            float tooltipX = (resolvedStyle.width - buttonName.resolvedStyle.width) / 2;
            float tooltipY = resolvedStyle.height - buttonName.resolvedStyle.height - 8;
            buttonName.style.left = tooltipX;
            buttonName.style.top = tooltipY;

            // Position the options button
            optionsButton.style.right = 8;
            optionsButton.style.top = 8;
        }

        private void HideElements()
        {
            buttonName.style.visibility = Visibility.Hidden;
            optionsButton.style.visibility = Visibility.Hidden;
        }

        public bool value
        {
            get => isToggledOn;
            set
            {
                if (isToggledOn != value)
                {
                    isToggledOn = value;
                    var changeEvent = ChangeEvent<bool>.GetPooled(!isToggledOn, isToggledOn);
                    changeEvent.target = this;
                    SendEvent(changeEvent);
                    ValueChanged?.Invoke(changeEvent);
                }
            }
        }

        public void Toggle()
        {
            value = !value;
            style.backgroundImage = value ? onIcon : offIcon;  // Switch icon
        }

        public void SetValueWithoutNotify(bool newValue)
        {
            isToggledOn = newValue;
            style.backgroundImage = new StyleBackground(isToggledOn ? onIcon : offIcon);  // Update icon
        }


        public EventCallback<ChangeEvent<bool>> ValueChanged { get; set; }

        private Texture2D CreateGrayscaleIcon(Texture2D original)
        {
            Texture2D grayTexture = new Texture2D(original.width, original.height);
            Color[] pixels = original.GetPixels();
            Color[] grayPixels = new Color[pixels.Length];

            for (int i = 0; i < pixels.Length; i++)
            {
                float grayValue = pixels[i].grayscale / 2;
                grayPixels[i] = new Color(grayValue, grayValue, grayValue, pixels[i].a);
            }

            grayTexture.SetPixels(grayPixels);
            grayTexture.Apply();
            return grayTexture;
        }
    }
}