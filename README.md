# Extended Managers Library 
<a href="LICENSE">
	<img src="https://img.shields.io/badge/license-MIT-green" />
</a>

#### Requires: Harmony 2.0.x ported over to Cities Skylines by [Bofomer](https://github.com/boformer) -- [Github](https://github.com/boformer/CitiesHarmony)
Extended Managers Library is a set of framework extension to remove the current limit of 64k props in Cities Skylines. It utilizes Harmony to patch the internal framework, and provides extension methods to ease the
transition from the original framework to this new framework.

Unlike Tree Anarchy, props have a hard limit of 64k due to the utilization of ushort for indexing instead of uint. This limitation makes it very hard to remove this limitation and the only viable solution is to replace the framework.
While replacing the framework, I have opted to enhance the performance of the rendering framework, because with more props, the impact on performance will be greater.

Current state of this mod is ALPHA, with the following targets to achieve BETA stage.

- [ ] Introduce native codes to improve performance
- [ ] Stabilize API framework


I need supporters/volunteers to help debug/code to make this mod even better. If you want to contribute, please contact me anytime.

Anyways, these codes are open to the public, as its a hobby of mine. If you wish to contribute to the codes, please join in.

