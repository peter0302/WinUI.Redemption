# WinUI "Redemption" #

WinUI 3 has come a long way to redeem itself from the failures of Metro, but it can still 
be maddeningly inflexible compared to WPF. This is a (currently) small 
compilation of workarounds that overcome some of WinUI 3's most significant remaining omissions.
Everything here is accomplished through managed code and public WinUI APIs. Virtually all of
the syntax quirks and hacks here are due to the overwhelming majority of WinUI's foundation being
sealed or otherwise not extensible; implementing them natively as part of WinUI with a more 
familiar/convenient syntax should be trivial with access to the internals and the right background 
in the native side of things.

## BindingStyle ##

Overcomes WinUI's inability to create usable bindings through `Style`s and `Setter`s 
(see https://github.com/microsoft/microsoft-ui-xaml/issues/8547).

Usage:

    <Style TargetType="Button">
        <Setter Property="wr:BindingStyle.Style">
            <Setter.Value>
                <wr:BindingStyle>
                    <wr:BindingSetter PropertyName="Content"
                                      Binding="{Binding RelativeSource={RelativeSource Mode=Self}, Path=Command.Name}" />
                </wr:BindingStyle>
            </Setter.Value>
        </Setter>
    </Style>

## MultiBinding ##

A MultiBinding framework that's (almost) as XAML friendly as the real thing
(see https://github.com/microsoft/microsoft-ui-xaml/issues/8334) and similar
internally to Xamarin Forms/MAUI's approach.

Usage:

      <CheckBox Content="All Are True"
                IsEnabled="False">
          <wr:MultiBinding.MultiBindings>
              <wr:MultiBindingCollection>
                  <wr:MultiBinding Converter="{StaticResource AllMustBeTrueMultiConverter}"
                                   PropertyName="IsChecked">
                      <wr:MultiBindingSource Binding="{Binding Option1}" />
                      <wr:MultiBindingSource Binding="{Binding Option2}" />
                      <wr:MultiBindingSource Binding="{Binding Option3}" />
                  </wr:MultiBinding>
              </wr:MultiBindingCollection>
          </wr:MultiBinding.MultiBindings>
      </CheckBox>

## Contributions ##

Contributions are welcome, particularly anything directed to making these solutions' XAML syntax more elegant/less cumbersome, but please raise an issue and confirm before making a PR.
