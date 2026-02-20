using System;
using System.Collections.Generic;

namespace SocialMediaSentimentApp;

public static class DemoData
{
    public static List<PostItem> BuildDemoPosts()
    {
        return new List<PostItem>
        {
            new() { Id = Guid.NewGuid(), Timestamp = new DateTime(2026,2,10,10,02,0), Platform = "Instagram", Text = "Checkout-nya gampang banget, keren! 👍" },
            new() { Id = Guid.NewGuid(), Timestamp = new DateTime(2026,2,10,11,14,0), Platform = "X/Twitter", Text = "Baru update malah error terus!!! parah" },
            new() { Id = Guid.NewGuid(), Timestamp = new DateTime(2026,2,11,9,33,0), Platform = "TikTok", Text = "Video promonya lucu, i like it" },
            new() { Id = Guid.NewGuid(), Timestamp = new DateTime(2026,2,11,20,05,0), Platform = "YouTube", Text = "Customer service lambat, kecewa :(" },
            new() { Id = Guid.NewGuid(), Timestamp = new DateTime(2026,2,12,8,10,0), Platform = "Instagram", Text = "UI-nya oke, tapi kadang lemot" },
            new() { Id = Guid.NewGuid(), Timestamp = new DateTime(2026,2,12,13,45,0), Platform = "X/Twitter", Text = "Not bad actually, works fine" },
            new() { Id = Guid.NewGuid(), Timestamp = new DateTime(2026,2,13,17,22,0), Platform = "Reddit", Text = "Harga naik lagi? expensive banget" },
            new() { Id = Guid.NewGuid(), Timestamp = new DateTime(2026,2,14,7,55,0), Platform = "TikTok", Text = "Mantap, pengiriman cepat, puas!" },
            new() { Id = Guid.NewGuid(), Timestamp = new DateTime(2026,2,14,22,10,0), Platform = "YouTube", Text = "Bug login belum fix, please fix" },
            new() { Id = Guid.NewGuid(), Timestamp = new DateTime(2026,2,15,15,00,0), Platform = "Instagram", Text = "Terima kasih! fitur baru membantu banget" },
        };
    }
}
