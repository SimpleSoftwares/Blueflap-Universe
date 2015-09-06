﻿Imports Windows.UI.Notifications
Imports Windows.UI.Xaml.Controls
''' <summary>
''' Page dédiée à la navigation web
''' </summary>
Public NotInheritable Class MainPage
    Inherits Page
    Public Sub New()
        Me.InitializeComponent()
        AddHandler Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested, AddressOf MainPage_BackRequested
    End Sub
    Private Sub MainPage_BackRequested(sender As Object, e As Windows.UI.Core.BackRequestedEventArgs)
        If Not Frame.CanGoBack Then

            If web.CanGoBack Then
                e.Handled = True
                web.GoBack()
            End If
        End If
    End Sub
    Private Sub Page_Loaded(sender As Object, e As RoutedEventArgs)

        Dim localSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings 'Permet l'accés aux paramètres

        If localSettings.Values("DarkThemeEnabled") = True Then 'Theme Sombre

            LightThemeMainPage.Stop() 'Solution provisoire parce que j'avais la flemme de chercher le code Argb
            BlackThemeMainPage.Begin() 'Idem (Je suis sûr qu'au final je ne le changerai pas parce que j'aurai des trucs plus importants à faire

            'Supprime la titlebar
            Dim titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView.TitleBar
            Dim v = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView()
            v.TitleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(255, 27, 27, 27)
            v.TitleBar.ButtonForegroundColor = Windows.UI.Colors.White
            v.TitleBar.ButtonInactiveBackgroundColor = Windows.UI.Color.FromArgb(255, 27, 27, 27)
            Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = True

            AdressBox.Foreground = New SolidColorBrush(Windows.UI.Colors.White)

        Else 'Theme Clair

            'Bon... Je vais pas recommenter la même chose... Débrouillez vous avec ce qu'il y a avant...
            BlackThemeMainPage.Stop()
            LightThemeMainPage.Begin()

            Dim titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView.TitleBar
            Dim v = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView()
            v.TitleBar.ButtonBackgroundColor = Windows.UI.Colors.WhiteSmoke
            v.TitleBar.ButtonForegroundColor = Windows.UI.Colors.DeepSkyBlue
            v.TitleBar.ButtonInactiveBackgroundColor = Windows.UI.Colors.WhiteSmoke
            Core.CoreApplication.GetCurrentView().TitleBar.ExtendViewIntoTitleBar = True

            Adressbar.Background = New SolidColorBrush(Windows.UI.Colors.WhiteSmoke)
            AdressBox.Foreground = New SolidColorBrush(Windows.UI.Colors.DimGray)
        End If


        AdressBox.IsEnabled = False 'Autre solution provisoire (qui va sans doute rester) parce que sinon l'adressbox obtient le focus à l'ouverture allez savoir pourquoi...
        AdressBox.IsEnabled = True

        BackForward()
        FirstLaunch()

        If AdressBox.Text = "about:blank" Then
            Try
                web.Navigate(New Uri(localSettings.Values("Homepage")))
            Catch
                Dim notificationXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02)
                Dim toeastElement = notificationXml.GetElementsByTagName("text")
                toeastElement(0).AppendChild(notificationXml.CreateTextNode("Erreur page d'accueil"))
                toeastElement(1).AppendChild(notificationXml.CreateTextNode("La page d'accueil définie est invalide. Rendez-vous dans les paramètres et vérifiez la configuration de votre page d'accueil."))
                Dim ToastNotification = New ToastNotification(notificationXml)
                ToastNotificationManager.CreateToastNotifier().Show(ToastNotification)
            End Try
        End If

    End Sub

    Private Sub BackForward()
        Back_Button.IsEnabled = web.CanGoBack
        Forward_Button.IsEnabled = web.CanGoForward
        StopEnabled.Stop()
        RefreshEnabled.Begin()

    End Sub

    Private Sub FirstLaunch() 'Définit les paramètres par défaut
        Dim localSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings

        If Not localSettings.Values("Config") = True Then
            localSettings.Values("A1") = "http://www.qwant.com/?q="
            localSettings.Values("A2") = ""
            localSettings.Values("SearchEngineIndex") = 1
        End If

    End Sub
    Private Sub web_NavigationCompleted(sender As WebView, args As WebViewNavigationCompletedEventArgs) Handles web.NavigationCompleted
        'Navigation terminée
        AdressBox.Text = web.Source.ToString
        Titlebox.Text = web.DocumentTitle
        loader.IsActive = False
        BackForward()
    End Sub

    Private Sub web_LoadCompleted(sender As Object, e As NavigationEventArgs) Handles web.LoadCompleted
        'Page chargée
        AdressBox.Text = web.Source.ToString
        Titlebox.Text = web.DocumentTitle
        loader.IsActive = False
        BackForward()
    End Sub

    Private Sub Home_button_Tapped(sender As Object, e As TappedRoutedEventArgs) Handles Home_button.Tapped
        'Clic sur le bouton home
        Dim localSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings

        Try
            web.Navigate(New Uri(localSettings.Values("Homepage")))
        Catch
            Dim notificationXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02)
            Dim toeastElement = notificationXml.GetElementsByTagName("text")
            toeastElement(0).AppendChild(notificationXml.CreateTextNode("Erreur page d'accueil"))
            toeastElement(1).AppendChild(notificationXml.CreateTextNode("La page d'accueil définie est invalide. Rendez-vous dans les paramètres et vérifiez la configuration de votre page d'accueil."))
            Dim ToastNotification = New ToastNotification(notificationXml)
            ToastNotificationManager.CreateToastNotifier().Show(ToastNotification)
        End Try
    End Sub

    Private Sub AdressBox_KeyDown(sender As Object, e As KeyRoutedEventArgs) Handles AdressBox.KeyDown
        Dim localSettings As Windows.Storage.ApplicationDataContainer = Windows.Storage.ApplicationData.Current.LocalSettings

        If (e.Key = Windows.System.VirtualKey.Enter) Then  'Permet de réagir à l'appui sur la touche entrée
            Dim textArray = AdressBox.Text.Split(" ")

            'Détermine si il s'agit d'une URL ou d'une recherche

            If (AdressBox.Text.Contains(".") = True And AdressBox.Text.Contains(" ") = False And AdressBox.Text.Contains(" .") = False And AdressBox.Text.Contains(". ") = False) Or textArray(0).Contains(":/") = True Or textArray(0).Contains(":\") Then
                If AdressBox.Text.Contains("http://") OrElse AdressBox.Text.Contains("https://") Then  'URL invalide si pas de http://
                    web.Navigate(New Uri(AdressBox.Text))
                Else
                    web.Navigate(New Uri("http://" + AdressBox.Text))
                End If

            Else
                Try
                    Dim Rech As String
                    Rech = AdressBox.Text
                    Dim s As String
                    s = Rech.ToString
                    s = s.Replace("+", "%2B")

                    If localSettings.Values("Custom_SearchEngine") = True Then
                        web.Navigate(New Uri(localSettings.Values("Cust1") + s + localSettings.Values("Cust2")))
                    Else
                        web.Navigate(New Uri(localSettings.Values("A1") + s + localSettings.Values("A2"))) 'Rechercher avec moteurs de recherche
                    End If
                Catch
                    Dim notificationXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText02)
                    Dim toeastElement = notificationXml.GetElementsByTagName("text")
                    toeastElement(0).AppendChild(notificationXml.CreateTextNode("Erreur moteur de recherche"))
                    toeastElement(1).AppendChild(notificationXml.CreateTextNode("Le moteur de recherche défini est invalide. Rendez-vous dans les paramètres et vérifiez la configuration de votre moteur de recherche."))
                    Dim ToastNotification = New ToastNotification(notificationXml)
                    ToastNotificationManager.CreateToastNotifier().Show(ToastNotification)
                End Try
            End If

            AdressBox.IsEnabled = False 'PROVISOIRE : Faire perdre le focus à la textbox
            AdressBox.IsEnabled = True
        End If
    End Sub

    Private Sub Strefresh_Button_Tapped(sender As Object, e As TappedRoutedEventArgs) Handles Strefresh_Button.Tapped
        If Strefresh_Button.Content = "" Then
            web.Refresh() 'Permet d'activer le ventilateur pour que votre PC ait moins chaud... Ahaha...Je suis trop drôle... En vrai ça sert juste à actualiser la page
            StopEnabled.Stop()
            RefreshEnabled.Begin()
        Else
            RefreshEnabled.Stop()
            StopEnabled.Begin()
            web.Stop()
        End If
        AdressBox.Text = web.Source.ToString
        Titlebox.Text = web.DocumentTitle
        loader.IsActive = False
        BackForward()
    End Sub

    Private Sub Back_Button_Tapped(sender As Object, e As TappedRoutedEventArgs) Handles Back_Button.Tapped
        web.GoBack() 'Permet de revenir à la page précédente
    End Sub

    Private Sub Forward_Button_Tapped(sender As Object, e As TappedRoutedEventArgs) Handles Forward_Button.Tapped
        web.GoForward() 'Revenir à la page suivante
    End Sub

    Private Sub web_NavigationStarting(sender As WebView, args As WebViewNavigationStartingEventArgs) Handles web.NavigationStarting
        loader.IsActive = True 'Les petites billes de chargement apparaissent quand une page se charge
        BackForward()
        RefreshEnabled.Stop()
        StopEnabled.Begin()
    End Sub

    Private Sub Paramètres_Button_Tapped(sender As Object, e As TappedRoutedEventArgs) Handles Paramètres_Button.Tapped
        Me.Frame.Navigate(GetType(Parametres)) 'Aller sur la page "Paramètres"
    End Sub
    Private Sub OnNewWindowRequested(sender As WebView, e As WebViewNewWindowRequestedEventArgs) Handles web.NewWindowRequested
        'Force l'ouverture dans Blueflap de liens censés s'ouvrir dans une nouvelle fenêtre
        web.Navigate(e.Uri)
        e.Handled = True
    End Sub
End Class
