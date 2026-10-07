using System.Runtime.CompilerServices;

// Lets TestApp (in this same repo, not a NuGet consumer) reach internal drag/drop-target types
// (DragService, IOverlayWindow, IDropTarget, DropTargetType) directly instead of via reflection, so
// DevFlow integration tests can query the live compass drop-target geometry during a real drag by
// DropTargetType rather than guessing screen offsets. Test-only surface area, not part of the public API.
// TestApp is signed with this same repo's sn.snk (see TestApp.csproj), so its public key matches
// AvalonDock's own - required because a strong-name-signed assembly can only grant InternalsVisibleTo
// to a friend identified by public key, not by name alone.
[assembly: InternalsVisibleTo("TestApp, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d59d8147eb2015ca98a92da860fd766d101271d8c2f545894870fd6183255737d79347bbf5250291ae75651e11501b7452ee003b80b936614cdda51db8eb6f8fde913e67d45395b480a992be17bf04744a7fe803ea131b925dcf84a73d22264352eca7c3fcf9387f3eee1d60ac7974f04866e6c72928dc0609abe341f92cbfb5")]
