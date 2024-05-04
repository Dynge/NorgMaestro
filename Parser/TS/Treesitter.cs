// TreeSitter is likely not going to happen in C# - i must write a Language Parser by hand instead
// To be fair - for my use case simply reading the file is probably quicker anyway.
//
// using static TreeSitter.Bindings.TSBindings;
//
// namespace NorgMaestro.TS
// {
//     // using TreeSitter;
//     internal unsafe class NorgLanguage
//     {
//         private const string NorgDllName = "tree-sitter-norg";
//         private const string NorgMetaDllName = "tree-sitter-norg";
//
//         [DllImport(NorgDllName)]
//         private static extern IntPtr tree_sitter_norg();
//
//         [DllImport(NorgMetaDllName)]
//         private static extern IntPtr tree_sitter_norg_meta();
//
//         // public static Language Create()
//         // {
//         //     return new Language(tree_sitter_norg());
//         // }
//         //
//         // public static Language CreateMeta()
//         // {
//         //     return new Language(tree_sitter_norg_meta());
//         // }
//         public static TSLanguage* Create()
//         {
//             return tree_sitter_norg();
//         }
//
//         public static Language CreateMeta()
//         {
//             return new Language(tree_sitter_norg_meta());
//         }
//     }
//
//     public class NorgSitter
//     {
//         public void MyMethod(string parameter)
//         {
//             Language norg = NorgLanguage.Create();
//             Language norg_meta = NorgLanguage.CreateMeta();
//             Parser norg_parser = new() { Language = norg };
//             Parser norg_meta_parser = new() { Language = norg_meta };
//             _ = norg_parser.Parse("").Root.;
//         }
//     }
//     public unsafe class NorgSitter
//     {
//         public void MyMethod()
//         {
//             var parser = parser_new();
//         }
//     }
// }
