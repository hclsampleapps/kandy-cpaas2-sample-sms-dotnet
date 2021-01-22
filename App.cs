using Nancy;
using Nancy.Extensions;
using Nancy.IO;
using Nancy.ModelBinding;
using Cpaas.Sdk;
using System.Collections.Generic;
using System;
using Cpaas.Sdk.resources;
using System.IO;
using Newtonsoft.Json;
using Nancy.Responses;

namespace sms {
  public class App : NancyModule {
    public static Client client = null;

    public App() {
      Get("/", args => {
        if (client == null) {
          Console.WriteLine("Inizializing ...");
          client = new Client(
            Environment.GetEnvironmentVariable("CLIENT_ID"),
            Environment.GetEnvironmentVariable("CLIENT_SECRET"),
            Environment.GetEnvironmentVariable("BASE_URL")
          );
          Console.WriteLine("Inizialization Complete ...");
        }
        return View["index.cshtml"];
      });

      Post("/send", args => {
        Sms sms = this.Bind();

        var response = client.conversation.CreateMessage(sms.number, new Dictionary<string, string> {
          ["type"] = client.conversation.types.SMS,
          ["senderAddress"] = Environment.GetEnvironmentVariable("PHONE_NUMBER"),
          ["message"] = sms.message
        });

        var alert = new Alert() {
          Message = "Success",
          Type = "success"
        };

        if (response.hasError) {
          alert.Message = ErrorMessageFrom(response);
          alert.Type = "error";
        }


        return View["index.cshtml", alert];
      });

      Post("/subscribe", args => {
        Subscription sub = this.Bind();

        var response = client.conversation.Subscribe(new Dictionary<string, string> {
          ["type"] = client.conversation.types.SMS,
           ["webhookURL"] = sub.webhook + "/webhook",
           ["destinationAddress"] = Environment.GetEnvironmentVariable("PHONE_NUMBER")
         });

        var alert = new Alert() {
          Message = "Subscription created.",
          Type = "success"
        };

        if (response.hasError) {
          alert.Message = ErrorMessageFrom(response);
          alert.Type = "error";
        }

        return View["index.cshtml", alert];
      });

      Post("/webhook", args => {
        var body = RequestStream.FromStream(Request.Body).AsString();

        var notification = client.notification.Parse(body);

        SetNotification(notification);

        return new TextResponse(HttpStatusCode.OK, "");
      });

      Get("/notifications", args => {
        var notifications = GetNotifications();

        return notifications;
      });
    }

    class Sms {
      public string number;
      public string message;
    }

    class Subscription {
      public string webhook;
    }

    // Helper methods
    string ErrorMessageFrom(dynamic obj) {
      return $"{obj.errorName}: {obj.errorMessage} ({obj.errorName})";
    }

    List<Notification.NotificationResponse> GetNotifications() {
      List<Notification.NotificationResponse> items = new List<Notification.NotificationResponse>();

      if (File.Exists("notification.json")) {
        using (StreamReader r = new StreamReader("notification.json")) {
          string json = r.ReadToEnd();
          items = JsonConvert.DeserializeObject<List<Notification.NotificationResponse>>(json);
        }
      }

      return items;
    }

    void SetNotification(Notification.NotificationResponse notification) {
      List<Notification.NotificationResponse> items = GetNotifications();

      items.Add(notification);

      if (!File.Exists("notification.js")) {
        File.CreateText("notification.js");
      }

      File.WriteAllText("notification.json", JsonConvert.SerializeObject(items));
    }
  }
}
