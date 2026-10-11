# Sample AWL files

These copies are the statement-list exports under `Triconex_Sample_Files`. The unit tests read the originals in that folder. This folder is the catalog next to the other notes.

| File here | Source | What it is |
| --- | --- | --- |
| `Logic-1.AWL` | `Triconex_Sample_Files\Logic-1.AWL` | `FUNCTION_BLOCK FB 500` plus `FUNCTION FC 0` and later functions. Bit logic, timers, latches, and `CALL FB 500`. This is the file the pattern tests were taken from. |
| `Logic-1 1.AWL` | `Triconex_Sample_Files\Logic-1 1.AWL` | Same export with a space in the file name. |
| `Siemens-Logic-1.AWL` | `Triconex_Sample_Files\Siemens Exported Backups\Logic-1.AWL` | Byte-for-byte the same equations as `Logic-1.AWL`. |
| `Siemens-Logic-test.AWL` | `...\Logic-test.AWL` | Only the FB 500 header and its four networks. Parsing yields six rows because the latch and the local-bit network each emit more than one equation. |
| `Siemens-Logic-2.AWL` | `...\Logic-2.AWL` | `DATA_BLOCK` initial values, not networks. |
| `Siemens-Logic-3.AWL` | `...\Logic-3.AWL` | Larger Siemens export. The file test checks that parsing finishes. |
| `FC2034.awl`, `FC2035.awl`, `FC2038.awl` | same names in `Triconex_Sample_Files` | Word logic (`L`, `T`, `UC`, `OPN`, `LAR2`) without a `FUNCTION` or `NETWORK` header. |

The zip and 7z archives from `Triconex_Sample_Files` are in `Archives`: `Logic-1.zip`, `Logic-2.zip`, `Logic-3.zip`, `Siemens Exported Backups.7z`, and `KCOP MSW.zip`. The STL manuals are in [Reference](../Reference/README.md). Saved parser workbooks are in [Testing/Saved Excel](../Testing/01-unit-tests.md).
