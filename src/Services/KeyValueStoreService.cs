using Grpc.Core;
using KeyValueStore;
using System.Collections.Concurrent;

public class KeyValueStoreService : KeyValue.KeyValueBase
{
    private readonly ConcurrentDictionary<string, string> _store = new ConcurrentDictionary<string, string>();

    public override Task<KeyValueResponse> Set(KeyValueRequest request, ServerCallContext context)
    {
        _store.AddOrUpdate(request.Key, request.Value, (key, oldValue) => request.Value);
        return Task.FromResult(new KeyValueResponse
        {
            Key = request.Key,
            Value = request.Value,
            Success = true
        });
    }

    public override Task<KeyValueResponse> Get(KeyRequest request, ServerCallContext context)
    {
        _store.TryGetValue(request.Key, out var value);
        return Task.FromResult(new KeyValueResponse
        {
            Key = request.Key,
            Value = value,
            Success = value != null
        });
    }

    public override Task<KeyValueResponse> Delete(KeyRequest request, ServerCallContext context)
    {
        var success = _store.TryRemove(request.Key, out var value);
        return Task.FromResult(new KeyValueResponse
        {
            Key = request.Key,
            Value = success ? value : null,
            Success = success
        });
    }
}