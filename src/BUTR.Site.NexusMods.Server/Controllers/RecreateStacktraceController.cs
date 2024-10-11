using BUTR.Authentication.NexusMods.Authentication;
using BUTR.CrashReport.Bannerlord.Parser;
using BUTR.CrashReport.Models;
using BUTR.Site.NexusMods.Server.Extensions;
using BUTR.Site.NexusMods.Server.Models;
using BUTR.Site.NexusMods.Server.Services;
using BUTR.Site.NexusMods.Server.Utils;
using BUTR.Site.NexusMods.Server.Utils.Http.ApiResults;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BUTR.Site.NexusMods.Server.Utils.BindingSources;

namespace BUTR.Site.NexusMods.Server.Controllers;

[ApiController, Route("api/v1/[controller]"), Authorize(AuthenticationSchemes = ButrNexusModsAuthSchemeConstants.AuthScheme)]
public sealed class RecreateStacktraceController : ApiControllerBase
{
    private readonly ILogger _logger;
    private readonly ICrashReporterClient _crashReporterClient;
    private readonly ISteamBinaryCache _steamBinaryCache;

    public RecreateStacktraceController(ILogger<RecreateStacktraceController> logger, ICrashReporterClient crashReporterClient, ISteamBinaryCache steamBinaryCache)
    {
        _logger = logger;
        _crashReporterClient = crashReporterClient;
        _steamBinaryCache = steamBinaryCache;
    }

    [HttpGet("Json")]
    [Produces("application/json")]
    public async Task<ApiResult<IEnumerable<RecreatedStacktrace>?>> GetJsonAsync([BindTenant] TenantId tenant, [FromQuery] CrashReportFileId id, CancellationToken ct)
    {
        if (!HttpContext.OwnsTenantGame())
            return ApiResultError("Game is not owned!", StatusCodes.Status401Unauthorized);

        string? crashReportContent;
        try
        {
            crashReportContent = await _crashReporterClient.GetCrashReportAsync(tenant, id, ct);
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return ApiOk(Enumerable.Empty<RecreatedStacktrace>());
        }

        if (string.IsNullOrEmpty(crashReportContent) || !CrashReportParser.TryParse(crashReportContent, out var version, out var crashReport, out var json))
            return ApiBadRequest("Invalid crash report!");

        var gameVersion = crashReport.Metadata.GameVersion;

        var assemblyFiles = await _steamBinaryCache.GetBranchAssemblyFilesAsync(gameVersion, ct);
        var recreatedStacktrace = RecreateStacktraceUtils.GetRecreatedStacktraceUnordered(assemblyFiles, crashReport, ct).ToList();

        var originalMethods = crashReport.EnhancedStacktrace.Select(x => x.OriginalMethod).OfType<MethodSimple>().ToList();
        var originalMethodNames = originalMethods.Select(x => x.GetFullName()).ToHashSet();
        originalMethodNames.ExceptWith(recreatedStacktrace.Select(x => x.Method));

        var originalMethodNamesArray = originalMethodNames.ToArray();
        var originalMethodIlOffsets = crashReport.EnhancedStacktrace.Where(x => x.OriginalMethod is not null).Select(x => (x.ILOffset, FullName: x.OriginalMethod!.GetFullName())).DistinctBy(x => x.FullName).ToDictionary(x => x.FullName, x => x.ILOffset);
        var recreatedStacktraceWithMissing = recreatedStacktrace.Concat(originalMethodNames.Select(x => new RecreatedStacktrace(x, $"No Code Available. IL Offset: {originalMethodIlOffsets[x]}", 1)))
            .OrderBy(x => Array.FindIndex(originalMethodNamesArray, y => y == x.Method));

        return ApiOk<IEnumerable<RecreatedStacktrace>>(recreatedStacktraceWithMissing);
    }

    [HttpGet("Html")]
    [Produces("text/plain")]
    public async Task<ActionResult<string>> GetHtmlAsync([BindTenant] TenantId tenant, [FromQuery] CrashReportFileId id, CancellationToken ct)
    {
        if (!HttpContext.OwnsTenantGame())
            return Unauthorized();

        string? crashReportContent;
        try
        {
            crashReportContent = await _crashReporterClient.GetCrashReportAsync(tenant, id, ct);
        }
        catch (HttpRequestException e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return Content(string.Empty, "text/html", Encoding.UTF8);
        }

        if (string.IsNullOrEmpty(crashReportContent) || !CrashReportParser.TryParse(crashReportContent, out var version, out var crashReport, out var json))
            return BadRequest();

        var gameVersion = crashReport.Metadata.GameVersion;

        var assemblyFiles = await _steamBinaryCache.GetBranchAssemblyFilesAsync(gameVersion, ct);
        var recreatedStacktrace = RecreateStacktraceUtils.GetRecreatedStacktraceUnordered(assemblyFiles, crashReport, ct).ToList();

        var currentMethods = crashReport.EnhancedStacktrace.Select(x => x.GetFullName()).ToHashSet();
        currentMethods.ExceptWith(recreatedStacktrace.Select(x => x.Method));

        var methods = crashReport.EnhancedStacktrace.Select(y => y.GetFullName()).ToArray();
        var ilOffsets = crashReport.EnhancedStacktrace.Select(x => (x.ILOffset, FullName: x.GetFullName())).DistinctBy(x => x.FullName).ToDictionary(x => x.FullName, x => x.ILOffset);
        var recreatedStacktraceWithMissing = recreatedStacktrace.Concat(currentMethods.Select(x => new RecreatedStacktrace(x, $"No Code Available. IL Offset: {ilOffsets[x]}", 1)))
            .OrderBy(x => Array.FindIndex(methods, y => y == x.Method));

        static string GetEnhancedStacktraceHtml(IEnumerable<RecreatedStacktrace> stacktrace)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<ol reversed>");
            foreach (var stacktraceEntry in stacktrace)
            {
                sb.Append("<li>")
                    .Append("<details>")
                    .Append($"<summary>{stacktraceEntry.Method}</summary>")
                    .Append($"<pre class=\"line-numbers\" data-line=\"{stacktraceEntry.LineNumber}\"><code class=\"language-cil\">{stacktraceEntry.CSharpWithIL}</code></pre>")
                    .Append("</details>")
                    .AppendLine("</li>");
            }
            sb.AppendLine("</ol>");
            return sb.ToString();
        }

        const string prismcss = """
	<style>
/* PrismJS 1.29.0
https://prismjs.com/download.html#themes=prism&languages=cil&plugins=line-highlight+line-numbers */
code[class*=language-],pre[class*=language-]{color:#000;background:0 0;text-shadow:0 1px #fff;font-family:Consolas,Monaco,'Andale Mono','Ubuntu Mono',monospace;font-size:1em;text-align:left;white-space:pre;word-spacing:normal;word-break:normal;word-wrap:normal;line-height:1.5;-moz-tab-size:4;-o-tab-size:4;tab-size:4;-webkit-hyphens:none;-moz-hyphens:none;-ms-hyphens:none;hyphens:none}code[class*=language-] ::-moz-selection,code[class*=language-]::-moz-selection,pre[class*=language-] ::-moz-selection,pre[class*=language-]::-moz-selection{text-shadow:none;background:#b3d4fc}code[class*=language-] ::selection,code[class*=language-]::selection,pre[class*=language-] ::selection,pre[class*=language-]::selection{text-shadow:none;background:#b3d4fc}@media print{code[class*=language-],pre[class*=language-]{text-shadow:none}}pre[class*=language-]{padding:1em;margin:.5em 0;overflow:auto}:not(pre)>code[class*=language-],pre[class*=language-]{background:#f5f2f0}:not(pre)>code[class*=language-]{padding:.1em;border-radius:.3em;white-space:normal}.token.cdata,.token.comment,.token.doctype,.token.prolog{color:#708090}.token.punctuation{color:#999}.token.namespace{opacity:.7}.token.boolean,.token.constant,.token.deleted,.token.number,.token.property,.token.symbol,.token.tag{color:#905}.token.attr-name,.token.builtin,.token.char,.token.inserted,.token.selector,.token.string{color:#690}.language-css .token.string,.style .token.string,.token.entity,.token.operator,.token.url{color:#9a6e3a;background:hsla(0,0%,100%,.5)}.token.atrule,.token.attr-value,.token.keyword{color:#07a}.token.class-name,.token.function{color:#dd4a68}.token.important,.token.regex,.token.variable{color:#e90}.token.bold,.token.important{font-weight:700}.token.italic{font-style:italic}.token.entity{cursor:help}
pre[data-line]{position:relative;padding:1em 0 1em 3em}.line-highlight{position:absolute;left:0;right:0;padding:inherit 0;margin-top:1em;background:hsla(24,20%,50%,.08);background:linear-gradient(to right,hsla(24,20%,50%,.1) 70%,hsla(24,20%,50%,0));pointer-events:none;line-height:inherit;white-space:pre}@media print{.line-highlight{-webkit-print-color-adjust:exact;color-adjust:exact}}.line-highlight:before,.line-highlight[data-end]:after{content:attr(data-start);position:absolute;top:.4em;left:.6em;min-width:1em;padding:0 .5em;background-color:hsla(24,20%,50%,.4);color:#f4f1ef;font:bold 65%/1.5 sans-serif;text-align:center;vertical-align:.3em;border-radius:999px;text-shadow:none;box-shadow:0 1px #fff}.line-highlight[data-end]:after{content:attr(data-end);top:auto;bottom:.4em}.line-numbers .line-highlight:after,.line-numbers .line-highlight:before{content:none}pre[id].linkable-line-numbers span.line-numbers-rows{pointer-events:all}pre[id].linkable-line-numbers span.line-numbers-rows>span:before{cursor:pointer}pre[id].linkable-line-numbers span.line-numbers-rows>span:hover:before{background-color:rgba(128,128,128,.2)}
pre[class*=language-].line-numbers{position:relative;padding-left:3.8em;counter-reset:linenumber}pre[class*=language-].line-numbers>code{position:relative;white-space:inherit}.line-numbers .line-numbers-rows{position:absolute;pointer-events:none;top:0;font-size:100%;left:-3.8em;width:3em;letter-spacing:-1px;border-right:1px solid #999;-webkit-user-select:none;-moz-user-select:none;-ms-user-select:none;user-select:none}.line-numbers-rows>span{display:block;counter-increment:linenumber}.line-numbers-rows>span:before{content:counter(linenumber);color:#999;display:block;padding-right:.8em;text-align:right}

	</style>
""";
        const string prismjs = """
    <script>
/* PrismJS 1.29.0
https://prismjs.com/download.html#themes=prism&languages=cil&plugins=line-highlight+line-numbers */
var _self="undefined"!=typeof window?window:"undefined"!=typeof WorkerGlobalScope&&self instanceof WorkerGlobalScope?self:{},Prism=function(e){var n=/(?:^|\s)lang(?:uage)?-([\w-]+)(?=\s|$)/i,t=0,r={},a={manual:e.Prism&&e.Prism.manual,disableWorkerMessageHandler:e.Prism&&e.Prism.disableWorkerMessageHandler,util:{encode:function e(n){return n instanceof i?new i(n.type,e(n.content),n.alias):Array.isArray(n)?n.map(e):n.replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/\u00a0/g," ")},type:function(e){return Object.prototype.toString.call(e).slice(8,-1)},objId:function(e){return e.__id||Object.defineProperty(e,"__id",{value:++t}),e.__id},clone:function e(n,t){var r,i;switch(t=t||{},a.util.type(n)){case"Object":if(i=a.util.objId(n),t[i])return t[i];for(var l in r={},t[i]=r,n)n.hasOwnProperty(l)&&(r[l]=e(n[l],t));return r;case"Array":return i=a.util.objId(n),t[i]?t[i]:(r=[],t[i]=r,n.forEach((function(n,a){r[a]=e(n,t)})),r);default:return n}},getLanguage:function(e){for(;e;){var t=n.exec(e.className);if(t)return t[1].toLowerCase();e=e.parentElement}return"none"},setLanguage:function(e,t){e.className=e.className.replace(RegExp(n,"gi"),""),e.classList.add("language-"+t)},currentScript:function(){if("undefined"==typeof document)return null;if("currentScript"in document)return document.currentScript;try{throw new Error}catch(r){var e=(/at [^(\r\n]*\((.*):[^:]+:[^:]+\)$/i.exec(r.stack)||[])[1];if(e){var n=document.getElementsByTagName("script");for(var t in n)if(n[t].src==e)return n[t]}return null}},isActive:function(e,n,t){for(var r="no-"+n;e;){var a=e.classList;if(a.contains(n))return!0;if(a.contains(r))return!1;e=e.parentElement}return!!t}},languages:{plain:r,plaintext:r,text:r,txt:r,extend:function(e,n){var t=a.util.clone(a.languages[e]);for(var r in n)t[r]=n[r];return t},insertBefore:function(e,n,t,r){var i=(r=r||a.languages)[e],l={};for(var o in i)if(i.hasOwnProperty(o)){if(o==n)for(var s in t)t.hasOwnProperty(s)&&(l[s]=t[s]);t.hasOwnProperty(o)||(l[o]=i[o])}var u=r[e];return r[e]=l,a.languages.DFS(a.languages,(function(n,t){t===u&&n!=e&&(this[n]=l)})),l},DFS:function e(n,t,r,i){i=i||{};var l=a.util.objId;for(var o in n)if(n.hasOwnProperty(o)){t.call(n,o,n[o],r||o);var s=n[o],u=a.util.type(s);"Object"!==u||i[l(s)]?"Array"!==u||i[l(s)]||(i[l(s)]=!0,e(s,t,o,i)):(i[l(s)]=!0,e(s,t,null,i))}}},plugins:{},highlightAll:function(e,n){a.highlightAllUnder(document,e,n)},highlightAllUnder:function(e,n,t){var r={callback:t,container:e,selector:'code[class*="language-"], [class*="language-"] code, code[class*="lang-"], [class*="lang-"] code'};a.hooks.run("before-highlightall",r),r.elements=Array.prototype.slice.apply(r.container.querySelectorAll(r.selector)),a.hooks.run("before-all-elements-highlight",r);for(var i,l=0;i=r.elements[l++];)a.highlightElement(i,!0===n,r.callback)},highlightElement:function(n,t,r){var i=a.util.getLanguage(n),l=a.languages[i];a.util.setLanguage(n,i);var o=n.parentElement;o&&"pre"===o.nodeName.toLowerCase()&&a.util.setLanguage(o,i);var s={element:n,language:i,grammar:l,code:n.textContent};function u(e){s.highlightedCode=e,a.hooks.run("before-insert",s),s.element.innerHTML=s.highlightedCode,a.hooks.run("after-highlight",s),a.hooks.run("complete",s),r&&r.call(s.element)}if(a.hooks.run("before-sanity-check",s),(o=s.element.parentElement)&&"pre"===o.nodeName.toLowerCase()&&!o.hasAttribute("tabindex")&&o.setAttribute("tabindex","0"),!s.code)return a.hooks.run("complete",s),void(r&&r.call(s.element));if(a.hooks.run("before-highlight",s),s.grammar)if(t&&e.Worker){var c=new Worker(a.filename);c.onmessage=function(e){u(e.data)},c.postMessage(JSON.stringify({language:s.language,code:s.code,immediateClose:!0}))}else u(a.highlight(s.code,s.grammar,s.language));else u(a.util.encode(s.code))},highlight:function(e,n,t){var r={code:e,grammar:n,language:t};if(a.hooks.run("before-tokenize",r),!r.grammar)throw new Error('The language "'+r.language+'" has no grammar.');return r.tokens=a.tokenize(r.code,r.grammar),a.hooks.run("after-tokenize",r),i.stringify(a.util.encode(r.tokens),r.language)},tokenize:function(e,n){var t=n.rest;if(t){for(var r in t)n[r]=t[r];delete n.rest}var a=new s;return u(a,a.head,e),o(e,a,n,a.head,0),function(e){for(var n=[],t=e.head.next;t!==e.tail;)n.push(t.value),t=t.next;return n}(a)},hooks:{all:{},add:function(e,n){var t=a.hooks.all;t[e]=t[e]||[],t[e].push(n)},run:function(e,n){var t=a.hooks.all[e];if(t&&t.length)for(var r,i=0;r=t[i++];)r(n)}},Token:i};function i(e,n,t,r){this.type=e,this.content=n,this.alias=t,this.length=0|(r||"").length}function l(e,n,t,r){e.lastIndex=n;var a=e.exec(t);if(a&&r&&a[1]){var i=a[1].length;a.index+=i,a[0]=a[0].slice(i)}return a}function o(e,n,t,r,s,g){for(var f in t)if(t.hasOwnProperty(f)&&t[f]){var h=t[f];h=Array.isArray(h)?h:[h];for(var d=0;d<h.length;++d){if(g&&g.cause==f+","+d)return;var v=h[d],p=v.inside,m=!!v.lookbehind,y=!!v.greedy,k=v.alias;if(y&&!v.pattern.global){var x=v.pattern.toString().match(/[imsuy]*$/)[0];v.pattern=RegExp(v.pattern.source,x+"g")}for(var b=v.pattern||v,w=r.next,A=s;w!==n.tail&&!(g&&A>=g.reach);A+=w.value.length,w=w.next){var E=w.value;if(n.length>e.length)return;if(!(E instanceof i)){var P,L=1;if(y){if(!(P=l(b,A,e,m))||P.index>=e.length)break;var S=P.index,O=P.index+P[0].length,j=A;for(j+=w.value.length;S>=j;)j+=(w=w.next).value.length;if(A=j-=w.value.length,w.value instanceof i)continue;for(var C=w;C!==n.tail&&(j<O||"string"==typeof C.value);C=C.next)L++,j+=C.value.length;L--,E=e.slice(A,j),P.index-=A}else if(!(P=l(b,0,E,m)))continue;S=P.index;var N=P[0],_=E.slice(0,S),M=E.slice(S+N.length),W=A+E.length;g&&W>g.reach&&(g.reach=W);var z=w.prev;if(_&&(z=u(n,z,_),A+=_.length),c(n,z,L),w=u(n,z,new i(f,p?a.tokenize(N,p):N,k,N)),M&&u(n,w,M),L>1){var I={cause:f+","+d,reach:W};o(e,n,t,w.prev,A,I),g&&I.reach>g.reach&&(g.reach=I.reach)}}}}}}function s(){var e={value:null,prev:null,next:null},n={value:null,prev:e,next:null};e.next=n,this.head=e,this.tail=n,this.length=0}function u(e,n,t){var r=n.next,a={value:t,prev:n,next:r};return n.next=a,r.prev=a,e.length++,a}function c(e,n,t){for(var r=n.next,a=0;a<t&&r!==e.tail;a++)r=r.next;n.next=r,r.prev=n,e.length-=a}if(e.Prism=a,i.stringify=function e(n,t){if("string"==typeof n)return n;if(Array.isArray(n)){var r="";return n.forEach((function(n){r+=e(n,t)})),r}var i={type:n.type,content:e(n.content,t),tag:"span",classes:["token",n.type],attributes:{},language:t},l=n.alias;l&&(Array.isArray(l)?Array.prototype.push.apply(i.classes,l):i.classes.push(l)),a.hooks.run("wrap",i);var o="";for(var s in i.attributes)o+=" "+s+'="'+(i.attributes[s]||"").replace(/"/g,"&quot;")+'"';return"<"+i.tag+' class="'+i.classes.join(" ")+'"'+o+">"+i.content+"</"+i.tag+">"},!e.document)return e.addEventListener?(a.disableWorkerMessageHandler||e.addEventListener("message",(function(n){var t=JSON.parse(n.data),r=t.language,i=t.code,l=t.immediateClose;e.postMessage(a.highlight(i,a.languages[r],r)),l&&e.close()}),!1),a):a;var g=a.util.currentScript();function f(){a.manual||a.highlightAll()}if(g&&(a.filename=g.src,g.hasAttribute("data-manual")&&(a.manual=!0)),!a.manual){var h=document.readyState;"loading"===h||"interactive"===h&&g&&g.defer?document.addEventListener("DOMContentLoaded",f):window.requestAnimationFrame?window.requestAnimationFrame(f):window.setTimeout(f,16)}return a}(_self);"undefined"!=typeof module&&module.exports&&(module.exports=Prism),"undefined"!=typeof global&&(global.Prism=Prism);
Prism.languages.cil={comment:/\/\/.*/,string:{pattern:/(["'])(?:\\(?:\r\n|[\s\S])|(?!\1)[^\\\r\n])*\1/,greedy:!0},directive:{pattern:/(^|\W)\.[a-z]+(?=\s)/,lookbehind:!0,alias:"class-name"},variable:/\[[\w\.]+\]/,keyword:/\b(?:abstract|ansi|assembly|auto|autochar|beforefieldinit|bool|bstr|byvalstr|catch|char|cil|class|currency|date|decimal|default|enum|error|explicit|extends|extern|famandassem|family|famorassem|final(?:ly)?|float32|float64|hidebysig|u?int(?:8|16|32|64)?|iant|idispatch|implements|import|initonly|instance|interface|iunknown|literal|lpstr|lpstruct|lptstr|lpwstr|managed|method|native(?:Type)?|nested|newslot|object(?:ref)?|pinvokeimpl|private|privatescope|public|reqsecobj|rtspecialname|runtime|sealed|sequential|serializable|specialname|static|string|struct|syschar|tbstr|unicode|unmanagedexp|unsigned|value(?:type)?|variant|virtual|void)\b/,function:/\b(?:(?:constrained|no|readonly|tail|unaligned|volatile)\.)?(?:conv\.(?:[iu][1248]?|ovf\.[iu][1248]?(?:\.un)?|r\.un|r4|r8)|ldc\.(?:i4(?:\.\d+|\.[mM]1|\.s)?|i8|r4|r8)|ldelem(?:\.[iu][1248]?|\.r[48]|\.ref|a)?|ldind\.(?:[iu][1248]?|r[48]|ref)|stelem\.?(?:i[1248]?|r[48]|ref)?|stind\.(?:i[1248]?|r[48]|ref)?|end(?:fault|filter|finally)|ldarg(?:\.[0-3s]|a(?:\.s)?)?|ldloc(?:\.\d+|\.s)?|sub(?:\.ovf(?:\.un)?)?|mul(?:\.ovf(?:\.un)?)?|add(?:\.ovf(?:\.un)?)?|stloc(?:\.[0-3s])?|refany(?:type|val)|blt(?:\.un)?(?:\.s)?|ble(?:\.un)?(?:\.s)?|bgt(?:\.un)?(?:\.s)?|bge(?:\.un)?(?:\.s)?|unbox(?:\.any)?|init(?:blk|obj)|call(?:i|virt)?|brfalse(?:\.s)?|bne\.un(?:\.s)?|ldloca(?:\.s)?|brzero(?:\.s)?|brtrue(?:\.s)?|brnull(?:\.s)?|brinst(?:\.s)?|starg(?:\.s)?|leave(?:\.s)?|shr(?:\.un)?|rem(?:\.un)?|div(?:\.un)?|clt(?:\.un)?|alignment|castclass|ldvirtftn|beq(?:\.s)?|ckfinite|ldsflda|ldtoken|localloc|mkrefany|rethrow|cgt\.un|arglist|switch|stsfld|sizeof|newobj|newarr|ldsfld|ldnull|ldflda|isinst|throw|stobj|stfld|ldstr|ldobj|ldlen|ldftn|ldfld|cpobj|cpblk|break|br\.s|xor|shl|ret|pop|not|nop|neg|jmp|dup|cgt|ceq|box|and|or|br)\b/,boolean:/\b(?:false|true)\b/,number:/\b-?(?:0x[0-9a-f]+|\d+)(?:\.[0-9a-f]+)?\b/i,punctuation:/[{}[\];(),:=]|IL_[0-9A-Za-z]+/};
!function(){if("undefined"!=typeof Prism&&"undefined"!=typeof document&&document.querySelector){var e,t="line-numbers",i="linkable-line-numbers",n=/\n(?!$)/g,r=!0;Prism.plugins.lineHighlight={highlightLines:function(o,u,c){var h=(u="string"==typeof u?u:o.getAttribute("data-line")||"").replace(/\s+/g,"").split(",").filter(Boolean),d=+o.getAttribute("data-line-offset")||0,f=(function(){if(void 0===e){var t=document.createElement("div");t.style.fontSize="13px",t.style.lineHeight="1.5",t.style.padding="0",t.style.border="0",t.innerHTML="&nbsp;<br />&nbsp;",document.body.appendChild(t),e=38===t.offsetHeight,document.body.removeChild(t)}return e}()?parseInt:parseFloat)(getComputedStyle(o).lineHeight),p=Prism.util.isActive(o,t),g=o.querySelector("code"),m=p?o:g||o,v=[],y=g.textContent.match(n),b=y?y.length+1:1,A=g&&m!=g?function(e,t){var i=getComputedStyle(e),n=getComputedStyle(t);function r(e){return+e.substr(0,e.length-2)}return t.offsetTop+r(n.borderTopWidth)+r(n.paddingTop)-r(i.paddingTop)}(o,g):0;h.forEach((function(e){var t=e.split("-"),i=+t[0],n=+t[1]||i;if(!((n=Math.min(b+d,n))<i)){var r=o.querySelector('.line-highlight[data-range="'+e+'"]')||document.createElement("div");if(v.push((function(){r.setAttribute("aria-hidden","true"),r.setAttribute("data-range",e),r.className=(c||"")+" line-highlight"})),p&&Prism.plugins.lineNumbers){var s=Prism.plugins.lineNumbers.getLine(o,i),l=Prism.plugins.lineNumbers.getLine(o,n);if(s){var a=s.offsetTop+A+"px";v.push((function(){r.style.top=a}))}if(l){var u=l.offsetTop-s.offsetTop+l.offsetHeight+"px";v.push((function(){r.style.height=u}))}}else v.push((function(){r.setAttribute("data-start",String(i)),n>i&&r.setAttribute("data-end",String(n)),r.style.top=(i-d-1)*f+A+"px",r.textContent=new Array(n-i+2).join(" \n")}));v.push((function(){r.style.width=o.scrollWidth+"px"})),v.push((function(){m.appendChild(r)}))}}));var P=o.id;if(p&&Prism.util.isActive(o,i)&&P){l(o,i)||v.push((function(){o.classList.add(i)}));var E=parseInt(o.getAttribute("data-start")||"1");s(".line-numbers-rows > span",o).forEach((function(e,t){var i=t+E;e.onclick=function(){var e=P+"."+i;r=!1,location.hash=e,setTimeout((function(){r=!0}),1)}}))}return function(){v.forEach(a)}}};var o=0;Prism.hooks.add("before-sanity-check",(function(e){var t=e.element.parentElement;if(u(t)){var i=0;s(".line-highlight",t).forEach((function(e){i+=e.textContent.length,e.parentNode.removeChild(e)})),i&&/^(?: \n)+$/.test(e.code.slice(-i))&&(e.code=e.code.slice(0,-i))}})),Prism.hooks.add("complete",(function e(i){var n=i.element.parentElement;if(u(n)){clearTimeout(o);var r=Prism.plugins.lineNumbers,s=i.plugins&&i.plugins.lineNumbers;l(n,t)&&r&&!s?Prism.hooks.add("line-numbers",e):(Prism.plugins.lineHighlight.highlightLines(n)(),o=setTimeout(c,1))}})),window.addEventListener("hashchange",c),window.addEventListener("resize",(function(){s("pre").filter(u).map((function(e){return Prism.plugins.lineHighlight.highlightLines(e)})).forEach(a)}))}function s(e,t){return Array.prototype.slice.call((t||document).querySelectorAll(e))}function l(e,t){return e.classList.contains(t)}function a(e){e()}function u(e){return!!(e&&/pre/i.test(e.nodeName)&&(e.hasAttribute("data-line")||e.id&&Prism.util.isActive(e,i)))}function c(){var e=location.hash.slice(1);s(".temporary.line-highlight").forEach((function(e){e.parentNode.removeChild(e)}));var t=(e.match(/\.([\d,-]+)$/)||[,""])[1];if(t&&!document.getElementById(e)){var i=e.slice(0,e.lastIndexOf(".")),n=document.getElementById(i);n&&(n.hasAttribute("data-line")||n.setAttribute("data-line",""),Prism.plugins.lineHighlight.highlightLines(n,t,"temporary ")(),r&&document.querySelector(".temporary.line-highlight").scrollIntoView())}}}();
!function(){if("undefined"!=typeof Prism&&"undefined"!=typeof document){var e="line-numbers",n=/\n(?!$)/g,t=Prism.plugins.lineNumbers={getLine:function(n,t){if("PRE"===n.tagName&&n.classList.contains(e)){var i=n.querySelector(".line-numbers-rows");if(i){var r=parseInt(n.getAttribute("data-start"),10)||1,s=r+(i.children.length-1);t<r&&(t=r),t>s&&(t=s);var l=t-r;return i.children[l]}}},resize:function(e){r([e])},assumeViewportIndependence:!0},i=void 0;window.addEventListener("resize",(function(){t.assumeViewportIndependence&&i===window.innerWidth||(i=window.innerWidth,r(Array.prototype.slice.call(document.querySelectorAll("pre.line-numbers"))))})),Prism.hooks.add("complete",(function(t){if(t.code){var i=t.element,s=i.parentNode;if(s&&/pre/i.test(s.nodeName)&&!i.querySelector(".line-numbers-rows")&&Prism.util.isActive(i,e)){i.classList.remove(e),s.classList.add(e);var l,o=t.code.match(n),a=o?o.length+1:1,u=new Array(a+1).join("<span></span>");(l=document.createElement("span")).setAttribute("aria-hidden","true"),l.className="line-numbers-rows",l.innerHTML=u,s.hasAttribute("data-start")&&(s.style.counterReset="linenumber "+(parseInt(s.getAttribute("data-start"),10)-1)),t.element.appendChild(l),r([s]),Prism.hooks.run("line-numbers",t)}}})),Prism.hooks.add("line-numbers",(function(e){e.plugins=e.plugins||{},e.plugins.lineNumbers=!0}))}function r(e){if(0!=(e=e.filter((function(e){var n,t=(n=e,n?window.getComputedStyle?getComputedStyle(n):n.currentStyle||null:null)["white-space"];return"pre-wrap"===t||"pre-line"===t}))).length){var t=e.map((function(e){var t=e.querySelector("code"),i=e.querySelector(".line-numbers-rows");if(t&&i){var r=e.querySelector(".line-numbers-sizer"),s=t.textContent.split(n);r||((r=document.createElement("span")).className="line-numbers-sizer",t.appendChild(r)),r.innerHTML="0",r.style.display="block";var l=r.getBoundingClientRect().height;return r.innerHTML="",{element:e,lines:s,lineHeights:[],oneLinerHeight:l,sizer:r}}})).filter(Boolean);t.forEach((function(e){var n=e.sizer,t=e.lines,i=e.lineHeights,r=e.oneLinerHeight;i[t.length-1]=void 0,t.forEach((function(e,t){if(e&&e.length>1){var s=n.appendChild(document.createElement("span"));s.style.display="block",s.textContent=e}else i[t]=r}))})),t.forEach((function(e){for(var n=e.sizer,t=e.lineHeights,i=0,r=0;r<t.length;r++)void 0===t[r]&&(t[r]=n.children[i++].getBoundingClientRect().height)})),t.forEach((function(e){var n=e.sizer,t=e.element.querySelector(".line-numbers-rows");n.style.display="none",n.innerHTML="",e.lineHeights.forEach((function(e,n){t.children[n].style.height=e+"px"}))}))}}}();

    </script>
""";

        var html = $"""
<html>
  <head>
    <title>Bannerlord Crash Report</title>
    <meta charset='utf-8'>
	{prismcss}
    {prismjs}
  </head>
  <body style='background-color: #ececec;'>
    {GetEnhancedStacktraceHtml(recreatedStacktraceWithMissing)}
  </body>
</html>
""";

        return Content(html, "text/html", Encoding.UTF8);
    }
}