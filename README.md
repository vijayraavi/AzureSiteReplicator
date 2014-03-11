Azure Site Replicator
===================

Azure Site Extension to replicate the content of one site to other sites using msdeploy.

Note: when running on a local machine, it may be necessary to delete the reg key `HKLM\Software\Wow6432Node\Microsoft\IIS Extensions\msdeploy\3\extensibility`, to avoid
running into the issue described [here](http://serverfault.com/questions/524848/msbuild-failing-on-build-looking-for-older-version-of-microsoft-data-tools-schem).
