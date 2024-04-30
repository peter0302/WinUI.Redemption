using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Markup;

using WinUI.Redemption.Internal;

namespace WinUI.Redemption
{
    /// <summary>
    /// Implements a WPF/MAUI-style multi-binding framework. Note currently
    /// only one-way bindings (from source to target) are supported.
    /// </summary>
    [ContentProperty(Name = nameof(Bindings))]
    public class MultiBinding
    {
        /// <summary>
        /// An attached <see cref="DependencyProperty"/> for assigning one or more
        /// <see cref="MultiBinding"/>s to a <see cref="FrameworkElement"/> via a
        /// <see cref="MultiBindingCollection"/> in XAML.
        /// </summary>
        public static readonly DependencyProperty MultiBindingsProperty = DependencyProperty.RegisterAttached(
            "MultiBindings",
            typeof(MultiBindingCollection),
            typeof(MultiBinding),
            new PropertyMetadata(null, (sender, e) =>
            {
                if (!(sender is FrameworkElement fe) ||
                    !(e.NewValue is MultiBindingCollection mbc))
                    return;
                foreach (var mb in mbc)
                {
                    var dp = (mb.PropertyOwner ?? fe.GetType()).TryGetDependencyProperty(mb.PropertyName);
                    if (dp == null)
                        continue;
                    Bind(fe, dp, mb);
                }
            }));
        public static MultiBindingCollection GetMultiBindings(DependencyObject obj)
        {
            return obj.GetValue(MultiBindingsProperty) as MultiBindingCollection;
        }
        public static void SetMultiBindings(DependencyObject obj, MultiBindingCollection value)
        {
            obj.SetValue(MultiBindingsProperty, value);
        }

        /// <summary>
        /// Used to create a <see cref="MultiBinding"/> binding through code.
        /// </summary>
        /// <param name="target">The target object.</param>
        /// <param name="targetProperty">The target property.</param>
        /// <param name="multiBinding">A <see cref="MultiBinding"/>
        /// instance. Note the <see cref="MultiBinding.Property"/>
        /// and related properties are unneeded as the 
        /// <paramref name="targetProperty"/> will always be used.
        /// </param>
        public static void Bind(
            FrameworkElement target, 
            DependencyProperty targetProperty,
            MultiBinding multiBinding)
        {
            var mbx = new MultiBindingExpression
            {
                MultiBinding = multiBinding,
                Target = target,
                TargetProperty = targetProperty,
            };
            mbx.Apply();
        }

        Collection<MultiBindingSource> _Bindings;

        /// <summary>
        /// A collection of <see cref="MultiBindingSource"/> instances.
        /// This is normally not set directly except when applying
        /// a <see cref="MultiBinding"/> in code using
        /// <see cref="MultiBinding.Bind(FrameworkElement, DependencyProperty, MultiBinding)"/>.
        /// </summary>
        public Collection<MultiBindingSource> Bindings
        {
            get => _Bindings ?? (_Bindings = new Collection<MultiBindingSource>());
            set => _Bindings = value;
        }

        /// <summary>
        /// Specifies the name of the target <see cref="DependencyProperty"/>.
        /// Either this and optionally 
        /// <see cref="PropertyOwner"/>
        /// must be populated when assigning in XAML as members of a 
        /// <see cref="MultiBindingCollection"/>.
        /// </summary>
        public string PropertyName 
        { 
            get; 
            set; 
        }

        /// <summary>
        /// In connection with <see cref="PropertyName"/>, used to
        /// unambiguously identify the owner of the target
        /// <see cref="DependencyProperty"/>. Typically used with
        /// attached properties since this class does not have 
        /// access to XAML namespaces.
        /// </summary>
        public Type PropertyOwner
        {
            get;
            set;
        }

        /// <summary>
        /// A required <see cref="IMultiValueConverter"/> instance 
        /// which receives the multiple values resulting from the
        /// <see cref="MultiBindingSource"/>s and converts them to
        /// a final target value for the target property.
        /// </summary>
        public IMultiValueConverter Converter
        {
            get;
            set;
        }

        /// <summary>
        /// An optional value that is passed along to
        /// <see cref="Converter"/> whenever any 
        /// <see cref="MultiBindingSource"/> value changes.
        /// </summary>
        public object ConverterParameter
        {
            get;
            set;
        }

        internal class ProxyCount
        {
            public int Count { get; set; }
        }

        internal class MultiBindingProxy
        {
            public Binding OriginalBinding
            {
                get;
                init;
            }

            public DependencyProperty ProxyProperty
            {
                get;
                init;
            }

            public object CurrentValue
            {
                get;
                set;
            }
        }

        private class MultiBindingExpression : INotifyPropertyChanged
        {
            object _FinalValue;
            [Obfuscation(Exclude = true)]
            public object FinalValue
            {
                get
                {
                    return _FinalValue;
                }
                set
                {
                    if (_FinalValue == value)
                        return;
                    _FinalValue = value;
                    this.PropertyChanged?.Invoke(
                        this, 
                        new PropertyChangedEventArgs(nameof(FinalValue)));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            internal MultiBinding MultiBinding { get; init; }

            internal FrameworkElement Target { get; set; }

            internal DependencyProperty TargetProperty { get; init; }

            internal MultiBindingProxy[] Proxies { get; set; }

            internal void Apply()
            {
                _isApplying = true;

                if (!_proxiesUsed.TryGetValue(this.Target, out var count))
                {
                    count = new ProxyCount();
                    _proxiesUsed.Add(this.Target, count);
                }

                this.Proxies = new MultiBindingProxy[this.MultiBinding.Bindings.Count];

                for (int i = 0; i < MultiBinding.Bindings.Count; i++)
                {
                    var proxy = new MultiBindingProxy
                    {
                        OriginalBinding = MultiBinding.Bindings[i].Binding,
                        ProxyProperty = GetProxyProperty(count.Count++),
                    };
                    var binding = MultiBinding.Bindings[i].Binding;
                    BindingOperations.SetBinding(
                        this.Target,
                        proxy.ProxyProperty,
                        new Binding
                        {
                            Source = binding.Source,
                            ElementName = binding.ElementName,
                            FallbackValue = binding.FallbackValue,
                            Mode = BindingMode.OneWay,
                            Path = binding.Path,
                            RelativeSource = binding.RelativeSource,
                            TargetNullValue = binding.TargetNullValue,
                            UpdateSourceTrigger = binding.UpdateSourceTrigger,
                            Converter = new MultiBindingProxyConverter(this, proxy),
                            ConverterLanguage = binding.ConverterLanguage,
                            ConverterParameter = null
                        });
                    this.Proxies[i] = proxy;
                }

                _isApplying = false;
                Reevaluate();
                BindingOperations.SetBinding(
                    this.Target,
                    this.TargetProperty,
                    new Binding
                    {
                        Source = this,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                        Path = new PropertyPath(nameof(FinalValue)),
                        Mode = BindingMode.OneWay,
                    });

                this.Target.Loaded += this.OnTargetLoaded;
                this.Target.Unloaded += this.OnTargetUnloaded;
            }

            internal void Reevaluate()
            {
                if (_isApplying || this.Target?.IsLoaded != true)
                    return;
                this.FinalValue = this.MultiBinding.Converter.Convert(
                    this.Proxies.Select(p => p.CurrentValue).ToArray(),
                    this.MultiBinding.ConverterParameter);
            }

            private void OnTargetLoaded(object sender, RoutedEventArgs e)
            {
                this.Reevaluate();
            }

            private void OnTargetUnloaded(object sender, RoutedEventArgs e)
            {
                this.Unapply();
            }

            private void Unapply()
            {
                // There's no need to do anything else because all the
                // bindings will just go away on their own.
                this.Target.Loaded -= this.OnTargetLoaded;
                this.Target.Unloaded -= this.OnTargetUnloaded;
                this.Target = null;
            }

            private static readonly GrowableArray<DependencyProperty> _proxyProps = new GrowableArray<DependencyProperty>(10);

            private static DependencyProperty GetProxyProperty(int i)
            {
                lock (_lock)
                {
                    var dp = _proxyProps[i];
                    if (dp != null)
                        return dp;
                    dp = DependencyProperty.Register(
                        $"MBProxy{i}",
                        typeof(object),
                        typeof(MultiBinding),
                        new PropertyMetadata(null, (sender, e) =>
                        {
                        }));
                    return _proxyProps[i] = dp;
                }
            }

            private static ConditionalWeakTable<FrameworkElement, ProxyCount> _proxiesUsed =
                new ConditionalWeakTable<FrameworkElement, ProxyCount>();

            private static object _lock = new object();

            private bool _isApplying;
        }

        private class MultiBindingProxyConverter : IValueConverter
        {
            MultiBindingExpression _expression;
            MultiBindingProxy _proxy;

            public MultiBindingProxyConverter(
                MultiBindingExpression expression,
                MultiBindingProxy proxy)
            {
                _proxy = proxy;
                _expression = expression;
            }

            public object Convert(object value, Type targetType, object parameter, string language)
            {
                // This will be called any time the source's value changes
                // so we need to re-evaluate the whole MB

                if (_proxy.OriginalBinding.Converter != null)
                {
                    // Use the original binding's converter if there was one
                    value = _proxy.OriginalBinding.Converter.Convert(
                        value,
                        targetType,
                        _proxy.OriginalBinding.ConverterParameter,
                        language);
                }

                _proxy.CurrentValue = value;
                _expression.Reevaluate();

                return value;
            }

            public object ConvertBack(object value, Type targetType, object parameter, string language)
            {
                throw new NotImplementedException();
            }
        }
    }
}
