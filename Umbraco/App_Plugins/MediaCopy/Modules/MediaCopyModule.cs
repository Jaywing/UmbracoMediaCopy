using Autofac;
using Autofac.Integration.WebApi;
using MediaCopy.Controllers;
using MediaCopy.Services;

namespace MediaCopy.Modules
{
    public class MediaCopyModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterApiControllers(typeof(MediaCopyController).Assembly);

            builder.RegisterType<MediaCopyService>().As<IMediaCopyService>();
        }
    }
} 