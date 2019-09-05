# Change log

## [0.1.0-preview] - 2019-07-30

### New Features

* New `GameObjectConversionSettings` class that we are using to help manage the various and growing settings that can tune a GameObject conversion.
* New ability to convert and export Assets, which is initially needed for Tiny.
  * Assets are discovered via `DeclareReferencedAsset` in the `GameObjectConversionDeclareObjectsGroup` phase and can then be converted by a System during normal conversion phases.
  * Assets can be marked for export and assigned a guid via `GameObjectConversionSystem.GetGuidForAssetExport`. During the System `GameObjectExportGroup` phase, the converted assets can be exported via `TryCreateAssetExportWriter`.
* `GetPrimaryEntity`, `HasPrimaryEntity`, and the new `TryGetPrimaryEntity` all now work on `UnityEngine.Object` instead of `GameObject` so that they can also query against Unity Assets.

### Upgrade guide

* Various GameObject conversion-related methods now receive a `GameObjectConversionSettings` object rather than a set of misc config params.
  * `GameObjectConversionSettings` has implicit constructors for common parameters such as `World`, so much existing code will likely just work.
  * Otherwise construct a `GameObjectConversionSettings`, configure it with the parameters you used previously, and send it in.
* `GameObjectConversionSystem`: `AddLinkedEntityGroup` is now `DeclareLinkedEntityGroup` (should auto-upgrade).
* The System group `GameObjectConversionDeclarePrefabsGroup` is now `GameObjectConversionDeclareObjectsGroup`. This cannot auto-upgrade but a global find&replace will fix it.
* `GameObjectConversionUtility.ConversionFlags.None` is gone, use 0 instead.

### Changes

### Fixes

## [0.0.1-preview.13] - 2019-05-24

### Changes

* Changing `entities` dependency to latest version (`0.0.12-preview.33`) 

## [0.0.1-preview.12] - 2019-05-16

### Fixes

* Adding/fixing `Equals` and `GetHashCode` for proxy components. 

## [0.0.1-preview.11] - 2019-05-01

Change tracking started with this version.

<!-- Template for version sections
## [0.0.0-preview.0]

### New Features


### Upgrade guide


### Changes


### Fixes
-->
