# Technical Design Description - Ariadna

Last reviewed: 2026-11-02. WPF is the sole application following user-confirmed
acceptance and retirement of WinForms and the migration utility.

## Purpose and boundaries

Ariadna is a personal Windows media catalog with one application instance and
five permanent tabs: Movies, Games, Library, Documentaries and XXX. Each tab
retains its filters, selection, scroll position and palette.

## Projects and architecture

- `Ariadna`: .NET 10 native WPF application, XAML views, observable models,
  virtualized poster rows, bounded asynchronous thumbnails and native editors.
- `Ariadna.Storage`: direct SQLite commands, explicit mapping and complete-entry
  transactions with recoverable external-image promotion.
- `Ariadna.Tests`: frontend workflow and virtualization tests.
- `Ariadna.Storage.Tests`: real-provider storage and recovery integration tests.

The desktop references storage directly. There is no WinForms, EF, SQL Server
client or conversion CLI in the solution. Collection-specific workflows remain
in the WPF application; SQL and persistence belong to storage. Startup enforces
one instance; subsequent launches activate it and defer tab switching during
modal editing.

## Persistence and configuration

`Ariadna/App.config` owns paths, tool locations and the TMDb API-key setting.
Keep credentials and private catalog contents out of examples and diagnostics.
`ARIADNA_CATALOG_PATH` and `ARIADNA_ASSET_ROOT` provide disposable test overrides.

Preserve catalog IDs, metadata, relationships and ignore entries. Poster PNGs
use extensionless IDs; Games previews append the configured suffix and 1–4,
and XXX has one screenshot. Shared portraits use `{personId}.png` at 200×300.
Missing portraits display `?`; image edits become durable only on Save.
Database and images form one matched recovery dataset. See
[SQLite operations](SQLITE-OPERATIONS.md) for storage and recovery constraints.

## WPF workflows

WPF hosts Movies, Games, Library, Documentaries and XXX in that order. `xxx`
selects the fifth tab; Ctrl+1 through Ctrl+5 select tabs directly and Ctrl+Tab /
Ctrl+Shift+Tab cycle all five, preserving each view's state. XXX is independent
of Movies: title, year, VR, Wishlist, shared cast identities (at most three),
poster and one screenshot. Its Details place Year and VR beside the compact
Cast section, with Screenshot below; translation, genres, directors, description
and audio languages are absent. Video metrics and playback follow Movies.
The palette uses near-black burgundy in Details and magenta-burgundy in browsing.

XXX uses additive `Xxx` and `XxxCast` tables within schema 2. Before extending an existing catalog,
WPF recovers pending image saves and creates an integrity-checked SQLite backup
beside the catalog (`.before-xxx-<unique-id>.sqlite`), then creates the tables
transactionally. Existing tables and asset files are untouched; that backup is
paired with the same existing assets. Matched backups include these tables and the `xxx` asset directory.
`XxxPostersRootPath` selects its image directory (empty defaults to `xxx` beside
the database); `DefaultXxxPath` and `DefaultXxxPathVR` configure recursive video discovery
(defaults: `a:\XXX\` and `a:\XXX_VR\`).
Its poster is extensionless `{id}`; the single screenshot uses `{id}{PreviewSuffix}1`,
with the same Save/cancel and recoverable asset promotion contract as Games.

WPF starts without a splash window. Recovery and the initial catalog query finish
before the main window is shown; remaining catalogs preload after its first render.
Startup errors retain the existing error dialog and unsuccessful exit code.
The tabs row reserves space at its far right for the main application's assembly
version (major.minor.patch), shown in semi-transparent black text. The label stays
visible across catalog switches and does not participate in keyboard or mouse input.
Main-window geometry is saved on accepted Closing and restored on the next launch
from the local application-data file `Ariadna/Wpf/window.json`. Normal restore
bounds are retained when closing minimized or maximized; the existing maximized
state is also persisted. Canceled closure and unshown windows leave saved geometry
unchanged. WPF tests initialize application resources without production startup.
Automated UI tests measure and arrange detached content on an STA dispatcher,
without native presentation sources. They exercise layout and model/action contracts;
activation, native keyboard focus, popup capture and modal interaction remain manual checks.
Startup and catalog tab activation focus the poster items list after restoring
the view state, rather than the Add entry button. Existing selection and scroll
positions are retained; unloaded views cannot receive deferred activation focus.
QuickList uses two columns in an 86-DIP sidebar on the active catalog background, with
24-DIP-high square buttons, thin white outlines and bold white letters.
Hover, keyboard focus and pressing provide a translucent-white highlight.
Letters read left to right, then downward. Button rows stay compact at the top,
leaving the catalog background below them. The sidebar scrolls independently
when letters exceed the available height;
letter clicks and keyboard activation retain the existing jump behavior.
Poster titles reserve 36 DIPs for two 18-DIP lines, wrapping with an ellipsis
when text exceeds two lines. Short titles remain centered in the same caption
area; poster height stays 380 DIPs and each card's total height is 438 DIPs.
Poster cards use square, one-DIP gray outlines and a vertical highlight gradient
from translucent white to dark gray. Hover is subtler than selection. Selecting
a card flashes its outline white three times over 420 ms, then leaves it gray;
deselection removes the animation. Images, captions and card dimensions remain
steady throughout highlighting.
The poster grid uses WPF item scrolling, with each virtualized item representing
one poster row. Startup selection, navigation, scrollbar movement and restored
tab positions keep the first visible row aligned to the viewport top; only the
bottom row may be partial. Each wheel notch moves one row, accumulating smaller
deltas and retaining selection. Resize anchoring preserves the first visible
entry's new row. Page Up/Page Down uses the rendered row height to determine how
many entries to move; row recycling and the final row's reachability are retained.
The poster grid's vertical scrollbar has a narrow catalog-colored track and a
rounded white thumb without arrow buttons. Thumb size follows the viewport,
with a minimum visible height of 50 DIPs. The thumb moves continuously under
the mouse and posters scroll live during dragging, with a whole top row.
The thumb retains its fractional drag position independently of WPF's rounded
content offset, then restores normal offset binding on release or unloading.
Track clicks retain immediate
page scrolling, and wheel/keyboard scrolling remains immediate. Its styles are
scoped to the poster list so editor and filter dropdown scrollbars are unaffected.
The shared filter panel retains its layout while showing only applicable fields:
Title, Wishlist, Recent, New and Genre in all catalogs; Series/Movies and
Directors/Actors in Movies; VR/Non-VR in Games; Authors in Library.
Library alone reveals the Subgenre label, selector and clear button after any
nonblank Genre is chosen. Changing Genre resets Subgenre; clearing Genre also
collapses that group. Filters and conditional visibility persist across tabs.
Each catalog's filter panel ends with a bold "Found entries: <count>" indicator,
bound to its displayed entries and updated after completed filter refreshes.
The compact filter toolbar uses the catalog palette, white separators and clear
icons, tinted borderless fields and outlined checkboxes after their labels.
Each field and its clear button wrap as one group when space is limited. Scoped
XAML styles preserve editable themed suggestion menus and visible keyboard focus
without changing editor controls. The plus button invokes existing discovery;
its context menu offers manual file and folder selection. Typing `+` while browsing
the catalog invokes the same Add entry button, including its disabled-state guard.
Plus characters in editable fields and Control/Alt/Windows combinations are not
treated as the shortcut.
Library's Genre dropdown contains the categories Languages, Literature,
Programming and Misc. Its Subgenre dropdown contains subjects for the selected
category (for example, Programming -> C++); stored subject tags are not added to
the category dropdown. The details editor retains the combined category/subject
tag list and custom stored tags. Storage and existing genre-query semantics are
unchanged.
The WPF Details editor reserves the left column for a large, uncropped poster.
Double-clicking the poster opens its image picker; there are no buttons below it.
The right column scrolls independently and contains themed metadata fields,
genre pictograms, description and portrait cards with name labels.
Directors and Cast remain separate for Movies; Library uses a full-width Authors
section. Details is owned by the main window and has no separate taskbar entry.
Each people section has a header `↓` button to paste clipboard names, normalize
word initials, skip duplicates and reuse stored portraits.
Each people header orders the buttons `↓`, then `+`. The `+` button adds a selected placeholder
named New Entry. Repeated clicks add separate cards; existing storage rules
consolidate identical names on save. People have no text fields or toolbars;
photo double-click and Ctrl+V remain available, and Delete removes a selected card.
Games retain VR, version and four preview slots; Documentaries omit people.
The Genres header has right-aligned clipboard `↓` and picker `...` buttons.
The old Edit genres section is removed. The floating icon picker opens beneath
selected genres, excludes already selected tags and confirms on double-click or
Enter. Escape or clicking outside dismisses it. Delete removes a selected genre;
Ctrl+V pastes names. Genre editing retains stored custom tags, clipboard input
and existing limits.
The fixed footer presents the editable path with a file/folder picker, separate
size, duration, resolution and bitrate icons, audio language flags with name
tooltips,
and Wishlist followed by Cancel and Save in a group anchored to the footer's
right edge. Media information stays left-aligned and wraps within its available
space without overlapping the action group. Details
scrollbars share the main window's rounded white thumb and unobtrusive track,
with normal continuous pixel scrolling. Media inspection runs asynchronously;
stale path results and results after close are discarded. Non-video entries show size only. Unknown
languages retain their names in tooltips with a generic icon. Bitrate converts
bits per second to whole-number Mbps. Footer groups wrap when space
is limited. Holding either Shift key changes Save to Ignore with gold text.
Shift release or window deactivation restores Save; confirming Ignore registers
the path without saving entry changes. Shift+Enter uses the same ignore workflow.
The manual TMDb button is removed; automatic lookup for newly discovered Movies
with a configured API key remains. Transactional saves retain existing metadata,
relationships and image behavior.

## Verification and references

Use synthetic data and disposable matched directories for write tests. Real SQLite
provider tests establish query and transaction behavior; UI changes also require
native Windows mouse/keyboard checks. Builds alone do not verify rendering,
external tools, live TMDb or clean-machine installation.

- [README](../README.md): restore, build, test, run and publish.
- [AGENTS](../AGENTS.md): operational and versioning rules.
- [Changelog](../CHANGELOG.md): dated implementation history.
- [Plan](PLAN.md): user-maintained upcoming work.
- [Memory](../MEMORY.md): historical handoff; older migration acceptance limits
  predate the user's 2026-11-02 WPF acceptance.
