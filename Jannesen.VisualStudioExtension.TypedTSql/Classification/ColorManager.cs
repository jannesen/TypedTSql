using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;

namespace Jannesen.VisualStudioExtension.TypedTSql.Classification
{
    [Export]
    public sealed class ColorManager: IDisposable
    {
        internal struct ClassificationColor: IEquatable<ClassificationColor>
        {
            public readonly Color?                  ForegroundColor;
            public readonly Color?                  BackgroundColor;

            public                                  ClassificationColor(Color? foregroundColor = null, Color? backgroundColor = null)
            {
                ForegroundColor = foregroundColor;
                BackgroundColor = backgroundColor;
            }

            public  static      bool            operator == (ClassificationColor p1, ClassificationColor p2)
            {
                return p1.ForegroundColor == p2.ForegroundColor &&
                       p1.BackgroundColor == p2.BackgroundColor;
            }
            public  static      bool            operator != (ClassificationColor p1, ClassificationColor p2)
            {
                return !(p1 == p2);
            }
            public  override    bool            Equals(object obj)
            {
                if (obj is ClassificationColor)
                    return this == (ClassificationColor)obj;

                return false;
            }
            public              bool            Equals(ClassificationColor o)
            {
                return this == o;
            }
            public  override    int             GetHashCode()
            {
                return (ForegroundColor.HasValue ? ForegroundColor.Value.GetHashCode() : 0) ^
                       (BackgroundColor.HasValue ? BackgroundColor.Value.GetHashCode() : 0);
            }
        }
        class DefaultClassificationColor
        {
            public readonly ClassificationColor     LightAndBlue;
            public readonly ClassificationColor     Dark;

            public                                  DefaultClassificationColor(ClassificationColor lightAndBlue, ClassificationColor dark)
            {
                LightAndBlue = lightAndBlue;
                Dark         = dark;
            }
        }

        private static readonly     Dictionary<string, DefaultClassificationColor>      _defaultColors = new Dictionary<string, DefaultClassificationColor>
                                    {
                                        { ClassificationTypes.Comment,         new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0, 128,   0)), new ClassificationColor(Color.FromRgb( 87, 166,  74))) },
                                        { ClassificationTypes.Name,            new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0,   0,   0)), new ClassificationColor(Color.FromRgb(220, 220, 220))) },
                                        { ClassificationTypes.Number,          new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(255,   0,   0)), new ClassificationColor(Color.FromRgb(214, 157, 133))) },
                                        { ClassificationTypes.String,          new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(255,   0,   0)), new ClassificationColor(Color.FromRgb(214, 157, 133))) },
                                        { ClassificationTypes.Operator,        new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(128, 128, 128)), new ClassificationColor(Color.FromRgb(180, 180, 180))) },
                                        { ClassificationTypes.Keyword,         new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0,   0, 255)), new ClassificationColor(Color.FromRgb( 86, 156, 214))) },
                                        { ClassificationTypes.LocalVariable,   new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0,   0,   0)), new ClassificationColor(Color.FromRgb(220, 220, 220))) },
                                        { ClassificationTypes.GlobalVariable,  new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0,   0,   0)), new ClassificationColor(Color.FromRgb(220, 220, 220))) },
                                        { ClassificationTypes.BuildIn,         new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(255,   0, 255)), new ClassificationColor(Color.FromRgb(201, 117, 213))) },
                                        { ClassificationTypes.Type,            new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0, 128, 168)), new ClassificationColor(Color.FromRgb( 78, 201, 176))) },
                                        { ClassificationTypes.Table,           new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0, 128, 168)), new ClassificationColor(Color.FromRgb( 78, 201, 176))) },
                                        { ClassificationTypes.View,            new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0, 128, 168)), new ClassificationColor(Color.FromRgb( 78, 201, 176))) },
                                        { ClassificationTypes.Function,        new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0, 128, 168)), new ClassificationColor(Color.FromRgb( 78, 201, 176))) },
                                        { ClassificationTypes.StoredProcedure, new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0, 128, 168)), new ClassificationColor(Color.FromRgb( 78, 201, 176))) },
                                        { ClassificationTypes.Parameter,       new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0,   0,   0)), new ClassificationColor(Color.FromRgb(220, 220, 220))) },
                                        { ClassificationTypes.Column,          new DefaultClassificationColor(new ClassificationColor(Color.FromRgb( 32, 132,   0)), new ClassificationColor(Color.FromRgb(184, 215, 163))) },
                                        { ClassificationTypes.UDTValue,        new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0,   0,   0)), new ClassificationColor(Color.FromRgb(220, 220, 220))) },
                                        { ClassificationTypes.Error,           new DefaultClassificationColor(new ClassificationColor(Color.FromRgb(  0,   0,   0)), new ClassificationColor(Color.FromRgb(255,   0,   0))) },
                                    };

        private                     VSPackage.ColorTheme                    _currentTheme;

        [ImportingConstructor]
        public ColorManager(IClassificationFormatMapService classificationFormatMapService, IClassificationTypeRegistryService classificationTypeRegistry)
        {
            _currentTheme                   = VSPackage.GetCurrentTheme();
//            VSColorTheme.ThemeChanged += _onThemeChanged;
        }

        public                      void                                    Dispose()
        {
//            VSColorTheme.ThemeChanged -= _onThemeChanged;
        }

        internal                    ClassificationColor                     GetDefaultColors(string category)
        {
            if (!_defaultColors.TryGetValue(category, out DefaultClassificationColor defaultColor))
                defaultColor = null;

            return _getDefaultClassificationColor(_currentTheme, defaultColor);
        }

/*
 * Not working correct with custom colors disable for now see also https://github.com/fsprojects/VisualFSharpPowerTools/pull/1480
        private                     void                                    _onThemeChanged(EventArgs e)
        {
            var newTheme = _getCurrentTheme();

            if (newTheme != Theme.Unknown && newTheme != _currentTheme) {
                _currentTheme = newTheme;

                var formatMap = _classificationFormatMapService.GetClassificationFormatMap(category: "text");

                try {
                    formatMap.BeginBatchUpdate();

                    foreach (var pair in _defaultColors) {
                        var classificationType = _classificationTypeRegistry.GetClassificationType(pair.Key);
                        FontColor   newColor   = _getDefaultFontColor(newTheme, pair.Value);
                        var         oldProp    = formatMap.GetTextProperties(classificationType);

                        var foregroundBrush    = newColor.ForegroundColor.HasValue ? new SolidColorBrush(newColor.ForegroundColor.Value) : null;
                        var backgroundBrush    = newColor.BackgroundColor.HasValue ? new SolidColorBrush(newColor.BackgroundColor.Value) : null;
                        var newProp            = TextFormattingRunProperties.CreateTextFormattingRunProperties(foregroundBrush,
                                                                                                                backgroundBrush,
                                                                                                                oldProp.Typeface,
                                                                                                                null,
                                                                                                                null,
                                                                                                                oldProp.TextDecorations,
                                                                                                                oldProp.TextEffects,
                                                                                                                oldProp.CultureInfo);
                        formatMap.SetTextProperties(classificationType, newProp);
                    }
                }
                finally {
                    formatMap.EndBatchUpdate();
                }
            }
        }
*/
        private static              ClassificationColor                     _getDefaultClassificationColor(VSPackage.ColorTheme theme, DefaultClassificationColor defaultColor)
        {
            if (defaultColor != null) {
                switch(theme) {
                case VSPackage.ColorTheme.Dark:
                    return defaultColor.Dark;

                case VSPackage.ColorTheme.Light:
                case VSPackage.ColorTheme.Blue:
                default:
                    return defaultColor.LightAndBlue;
                }
            }
            else {
                switch(theme) {
                case VSPackage.ColorTheme.Dark:
                    return new ClassificationColor(Colors.White);

                case VSPackage.ColorTheme.Light:
                case VSPackage.ColorTheme.Blue:
                default:
                    return new ClassificationColor(Colors.Black);
                }
            }
        }
    }
}
