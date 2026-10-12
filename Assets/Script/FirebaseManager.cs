using UnityEngine;
using Firebase;
using Firebase.Messaging;
using System;
using System.Collections.Generic;
using System.Collections; // NUOVO: Serve per le Coroutine dei permessi

#if UNITY_ANDROID
using Unity.Notifications.Android;
using UnityEngine.Android; // NUOVO: Serve per chiedere il permesso su Android 13+
#elif UNITY_IOS
using Unity.Notifications.iOS;
#endif

public class FirebaseManager : MonoBehaviour
{
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();

    void Start()
    {
        // NUOVO: Avvia la richiesta dei permessi prima o durante l'inizializzazione
        StartCoroutine(RequestNotificationPermission());

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                // NUOVO: Spostiamo l'inizializzazione sul Thread principale tramite la coda
                lock (_executionQueue)
                {
                    _executionQueue.Enqueue(() => InitializeFirebaseMessaging());
                }
            }
            else
            {
                Debug.LogError($"Impossibile risolvere le dipendenze di Firebase: {dependencyStatus}");
            }
        });
    }

    void Update()
    {
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    // NUOVO METODO: Gestisce la richiesta dei permessi di sistema
    private IEnumerator RequestNotificationPermission()
    {
        /*
#if UNITY_ANDROID
        // Android 13 (API 33) richiede il permesso esplicito POST_NOTIFICATIONS
        //if (SystemInfo.operatingSystemInstallerName != "AmazonAppstore") // Controllo generico
        //{
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
        //}
        yield return null;
#elif UNITY_IOS
        // Richiesta permesso su iOS
        var authorizationOption = AuthorizationOption.Alert | AuthorizationOption.Badge | AuthorizationOption.Sound;
        using (var req = new AuthorizationRequest(authorizationOption, true))
        {
            while (!req.IsCompleted)
            {
                yield return null;
            }
        }
#else
        yield return null;
#endif
*/
        yield return null;
    }

    void InitializeFirebaseMessaging()
    {
        FirebaseMessaging.MessageReceived += OnMessageReceived;
        FirebaseMessaging.TokenReceived += OnTokenReceived;

#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel()
        {
            Id = "foreground_channel",
            Name = "Notifiche In-App",
            Importance = Importance.High,
            Description = "Visualizza le notifiche quando l'app è aperta"
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif

        // NUOVO: Iscrivi il dispositivo al topic globale usato dal tuo script PHP
        FirebaseMessaging.SubscribeAsync("global_news").ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Iscritto con successo al topic 'global_news'");
            }
            else
            {
                Debug.LogError("Errore durante l'iscrizione al topic.");
            }
        });

        Debug.Log("Firebase Messaging Inizializzato correttamente.");
    }

    private void OnTokenReceived(object sender, TokenReceivedEventArgs token)
    {
        Debug.Log($"Token di registrazione ricevuto: {token.Token}");
    }

    private void OnMessageReceived(object sender, MessageReceivedEventArgs e)
    {
        Debug.Log("Nuova notifica ricevuta!");
        if (e.Message.Notification != null)
        {
            string titolo = e.Message.Notification.Title;
            string corpo = e.Message.Notification.Body;

            lock (_executionQueue)
            {
                _executionQueue.Enqueue(() =>
                {
#if UNITY_ANDROID
                    MostraNotificaAndroid(titolo, corpo);
#elif UNITY_IOS
                    MostraNotificaiOS(titolo, corpo);
#endif
                });
            }
        }
    }

#if UNITY_ANDROID
    private void MostraNotificaAndroid(string titolo, string corpo)
    {
        var notificaAndroid = new AndroidNotification();
        notificaAndroid.Title = titolo;
        notificaAndroid.Text = corpo;
        notificaAndroid.SmallIcon = "icon_0";
        notificaAndroid.FireTime = DateTime.Now;

        AndroidNotificationCenter.SendNotification(notificaAndroid, "foreground_channel");
    }
#endif

#if UNITY_IOS
    private void MostraNotificaiOS(string titolo, string corpo)
    {
        var notificaiOS = new iOSNotification();
        notificaiOS.Identifier = Guid.NewGuid().ToString();
        notificaiOS.Title = titolo;
        notificaiOS.Body = corpo;
        notificaiOS.ShowInForeground = true;
        notificaiOS.Trigger = new iOSNotificationTimeIntervalTrigger()
        {
            TimeInterval = new TimeSpan(0, 0, 1),
            Repeats = false
        };

        iOSNotificationCenter.ScheduleNotification(notificaiOS);
    }
#endif

    void OnDestroy()
    {
        FirebaseMessaging.MessageReceived -= OnMessageReceived;
        FirebaseMessaging.TokenReceived -= OnTokenReceived;
    }
}
