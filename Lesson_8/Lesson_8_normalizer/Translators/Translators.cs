using Translator.Translators;
namespace Translators
{
    internal class TranslatorRunner
    {
        internal void Run()
        {
            new KlmTranslator().Setup();
            new SasTranslator().Setup();
            new SwTranslator().Setup();
        }
    }
}