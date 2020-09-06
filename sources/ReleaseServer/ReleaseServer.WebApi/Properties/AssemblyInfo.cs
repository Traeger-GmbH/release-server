using System.Runtime.CompilerServices;

//Make the internal classes accessible in the test assembly (also for mocking)
[assembly:InternalsVisibleTo("ReleaseServer.WebApi.Test")]
[assembly:InternalsVisibleTo("DynamicProxyGenAssembly2")]