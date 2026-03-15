using SPage.Domain.Common;
using SPage.Domain.Entities.Blog;

namespace SPage.Domain.Events;

public sealed class PostPublishedEvent : BaseEvent
{
    public Post Post { get; }
    public PostPublishedEvent(Post post) => Post = post;
}
