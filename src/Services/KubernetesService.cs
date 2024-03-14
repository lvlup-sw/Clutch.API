using k8s;

namespace StepNet.Services
{
    public class KubernetesService
    {
        var kube = new Kubernetes(KubernetesClientConfiguration.BuildDefaultConfig());
    }
}
