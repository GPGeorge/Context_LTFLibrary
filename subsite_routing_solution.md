# Subsite Routing: Complete Solution

**Problem Solved:** Authentication redirects and navigation going to `gpcdata.com/login` instead of `gpcdata.com/LTFCatalog/login`

**Date:** September 2025  
**Project:** LTFCatalog Blazor Server Application  
**Environment:** IIS subsite configuration

## Problem Summary

When adding new pages (Login.razor, Admin.razor) to the LTFCatalog application running as a subsite, authentication redirects and navigation were bypassing the subsite path and going to the root domain instead.

**Symptoms:**
- Authentication redirects went to `gpcdata.com/login` instead of `gpcdata.com/LTFCatalog/login`
- Internal navigation links escaped the subsite
- 404 errors on login/admin pages
- Blazor SignalR circuit connection errors

## Root Cause

**This was a systematic issue, not an isolated authentication problem.** Absolute paths throughout the codebase were bypassing the subsite configuration.

## Complete Solution

### 1. Program.cs Configuration

**Middleware Order (Critical):**
```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UsePathBase("/LTFCatalog");    // Must come before UseRouting
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapBlazorHub();
app.MapControllers();
app.MapFallbackToPage("/_Host");
```

**Cookie Authentication with Custom Redirect:**
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = null;  // Disable automatic redirects
    options.LogoutPath = "/LTFCatalog/logout";
    options.AccessDeniedPath = "/LTFCatalog/access-denied";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.Cookie.SameSite = SameSiteMode.Lax;
    
    // Custom redirect logic to handle subsite
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.Redirect("/LTFCatalog/login");
        return Task.CompletedTask;
    };
});
```

### 2. _Host.cshtml Configuration

**Keep standard base href:**
```html
<base href="~/" />
```

**Note:** We tried `<base href="/LTFCatalog/" />` but it caused Blazor SignalR circuit errors. The `UsePathBase` configuration handles the subsite routing without needing to change this.

### 3. web.config IIS Configuration

**Rewrite rules for subsite:**
```xml
<rewrite>
  <rules>
    <rule name="Blazor Routes" stopProcessing="true">
      <match url=".*" />
      <conditions logicalGrouping="MatchAll">
        <add input="{REQUEST_FILENAME}" matchType="IsFile" negate="true" />
        <add input="{REQUEST_FILENAME}" matchType="IsDirectory" negate="true" />
        <add input="{REQUEST_URI}" pattern="^/(api|hubs)" negate="true" />
      </conditions>
      <action type="Rewrite" url="/LTFCatalog/" />
    </rule>
  </rules>
</rewrite>
```

### 4. Navigation Code Fixes

**The Critical Discovery:** Every absolute path in the codebase bypassed the subsite configuration.

**❌ Wrong (Absolute Paths):**
```csharp
// These all bypass UsePathBase and go to root domain
href="/login"
href="/admin"
Navigation.NavigateTo("/admin")
Navigation.NavigateTo("/")
```

**✅ Correct (Relative Paths):**
```csharp
// These work with UsePathBase and stay in subsite
href="login"
href="admin"
Navigation.NavigateTo("admin")
Navigation.NavigateTo("") // or "../" for parent
```

**Files Requiring Fixes:**
- AdminNavigation.razor - All href and NavigateTo calls
- Admin.razor - LogoutAsync navigation
- Login.razor - Success navigation after login
- Any other components with navigation links

**@page Directives Stay Absolute:**
```csharp
@page "/login"    // ✅ These work correctly with UsePathBase
@page "/admin"    // ✅ Server-side routing handles subsite automatically
```

## Key Learnings

### 1. Systematic Nature of Problem
This wasn't just an authentication issue - it was a comprehensive routing problem affecting every navigation call. Always do a project-wide search for absolute paths when dealing with subsite routing.

### 2. Client vs Server Routing
- **Server-side routes** (`@page` directives) work correctly with `UsePathBase`
- **Client-side navigation** (`href`, `NavigateTo`) requires relative paths to respect subsite configuration

### 3. Case Sensitivity Matters
URLs are case-sensitive in web environments. `UsePathBase("/LTFCatalog")` must match the exact case of your actual URL.

### 4. Middleware Order is Critical
`UsePathBase` must come before `UseRouting()` and authentication middleware. Wrong order causes authentication redirects to fail.

### 5. IIS Configuration Required
For subsite deployments, web.config rewrite rules are essential to handle URL routing at the IIS level.

## Testing Checklist

After implementing these fixes, verify:

- [ ] Direct URL access to `/LTFCatalog/login` works
- [ ] Authentication redirects go to correct subsite URLs
- [ ] All internal navigation stays within subsite
- [ ] No Blazor SignalR circuit errors
- [ ] Static files (CSS, images, JS) load correctly
- [ ] Logout redirects to correct location

## Search Patterns for Future Issues

When adding new pages or components, search for these patterns:

**In .razor files:**
```
href="/
Navigation.NavigateTo("/
src="/
action="/
```

**In C# code:**
```
"/admin"
"/login"
Response.Redirect("/
```

## Prevention

1. **Always use relative paths** for internal navigation
2. **Test new pages immediately** after creation
3. **Include routing in code review** checklist
4. **Document subsite configuration** for team members

## Related Issues

- Blazor SignalR circuit failures when base href is modified
- IIS URL rewrite conflicts with ASP.NET Core routing
- Case sensitivity in URL paths for production environments

---

**Success Pattern:** The combination of proper middleware order, custom authentication events, and systematic elimination of absolute paths resolves all subsite routing issues for Blazor Server applications deployed as IIS subsites.