using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtkBlazorApp.Core.Data.Cdek;

public record CdekWebhookRegsterRequest(string url, string type);
