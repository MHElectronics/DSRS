using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace DSRSystem.Components;

public abstract class BasePageComponent : ComponentBase
{

    [Inject]
    protected IJSRuntime JSRuntime { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            // run some JS function(s) here 
            await JSRuntime.InvokeVoidAsync("SetThemeModes");
        }
    }
}
