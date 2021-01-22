using Nancy;
using Nancy.Bootstrapper;
using Nancy.Session;
using Nancy.TinyIoc;
using dotenv.net;

namespace fa {
  public class Bootstrapper : DefaultNancyBootstrapper {
    protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines) {
      CookieBasedSessions.Enable(pipelines);
    }
  }
}
