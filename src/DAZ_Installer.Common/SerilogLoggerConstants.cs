using Serilog.Templates;

namespace DAZ_Installer
{
    internal static class SerilogLoggerConstants
    {
        internal static ExpressionTemplate Template = new ExpressionTemplate("[{@t:yyyy-MM-dd HH:mm:ss} {@l:u3}] [Thread {ThreadId}]" +
                                                                            "{#if SourceContext is not null} [{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}]{#end}" +
                                                                            //"{#each _, v in Rest()}" + " [{v}]{#end}" +
                                                                            "{#if Archive is not null} [{Archive}]{#end}" +
                                                                            "{#if File is not null} [{File}]{#end}" +
                                                                            ": {@m}\n{@x}");
        internal static ExpressionTemplate LoggerTemplate = new ExpressionTemplate("[{@l:u3}]" +
                                                                            "{#if SourceContext is not null} [{Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)}]{#end}" +
                                                                            //"{#each _, v in Rest()}" + " [{v}]{#end}" +
                                                                            "{#if Archive is not null} [{Archive}]{#end}" +
                                                                            "{#if File is not null} [{File}]{#end}" +
                                                                            ": {@m}\n{@x}");

    }
}
