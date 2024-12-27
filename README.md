# LargeFileGeneratorAndSorter
This repository contains two C# utilities: one generates large text files of lines 'Number. String' (up to ~100GB), including duplicates, and another sorts them by string first, then number. Sorting relies on chunk-based external merge for efficiency in limited memory. Performance and correctness are key, with a focus on large-scale data handling.
