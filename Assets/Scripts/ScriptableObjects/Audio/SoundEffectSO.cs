// using MyBox;
// using UnityEditor;
// using UnityEngine;
//
// namespace ScriptableObjects
// {
//     [CreateAssetMenu(fileName = "New Sound Effect", menuName = "Audio/Sound Effect")]
//     public class SoundEffectSO : ScriptableObject
//     {
//         private static readonly float SEMITONES_TO_PITCH_CONVERSION_UNIT = 1.05946f;
//
//         private enum SoundClipPlayOrder
//         {
//             Random,
//             InOrder,
//             Reverse
//         }
//
//         [Separator("General")]
//         [MustBeAssigned] public AudioClip[] clips;
//         [MinMaxRange(0, 1)] public RangedFloat volume = new RangedFloat(0.5f, 0.5f);
//
//         [Separator("Pitch")]
//         public bool useSemitones;
//         [ConditionalField(nameof(useSemitones), false, true)]
//         [MinMaxRange(-10, 10)] public RangedInt semitones = new RangedInt(0, 0);
//
//         [ConditionalField(nameof(useSemitones), false, false)]
//         [MinMaxRange(0, 3)] public RangedFloat pitch = new RangedFloat(1.0f, 1.0f);
//
//         [SerializeField] private SoundClipPlayOrder playOrder;
//         [SerializeField] private int playIndex;
//
// #if UNITY_EDITOR
//         private AudioSource _previewer;
//
//         private void OnEnable() {
//             _previewer = EditorUtility.CreateGameObjectWithHideFlags("AudioPreview", HideFlags.HideAndDontSave, typeof(AudioSource))
//                 .GetComponent<AudioSource>();
//         }
//
//         private void OnDisable() {
//             DestroyImmediate(_previewer.gameObject);
//         }
//
//         [ButtonMethod]
//         private void PlayPreview() {
//             Play(_previewer);
//         }
//
//         [ButtonMethod]
//         private void StopPreview() {
//             _previewer.Stop();
//         }
//
//         private void OnValidate() {
//             SyncPitchAndSemitones();
//         }
// #endif
//
//         public void SyncPitchAndSemitones() {
//             if (useSemitones)
//             {
//                 pitch.Min = Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, semitones.Min);
//                 pitch.Max = Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, semitones.Max);
//             }
//             else
//             {
//                 semitones.Min = Mathf.RoundToInt(Mathf.Log10(pitch.Min) / Mathf.Log10(SEMITONES_TO_PITCH_CONVERSION_UNIT));
//                 semitones.Max = Mathf.RoundToInt(Mathf.Log10(pitch.Max) / Mathf.Log10(SEMITONES_TO_PITCH_CONVERSION_UNIT));
//             }
//         }
//
//         private int RepeatCheck(int previousIndex, int range) {
//             int index = Random.Range(0, range);
//
//             while (index == previousIndex) index = Random.Range(0, range);
//             return index;
//         }
//
//         private AudioClip GetRandomAudioClip() {
//             // Get current clip.
//             AudioClip clip = clips[playIndex >= clips.Length ? 0 : playIndex];
//
//             // Find next clip.
//             switch (playOrder)
//             {
//                 case SoundClipPlayOrder.InOrder:
//                     playIndex = (playIndex + 1) % clips.Length;
//                     break;
//                 case SoundClipPlayOrder.Random:
//                     playIndex = RepeatCheck(playIndex, clips.Length);
//                     break;
//                 case SoundClipPlayOrder.Reverse:
//                     playIndex = (playIndex + clips.Length - 1) % clips.Length;
//                     break;
//             }
//
//             return clip;
//         }
//
//         public AudioSource Play(AudioSource audioSourceParam = null) {
//             if (clips.Length == 0)
//             {
//                 Debug.LogWarning($"Missing sound clips for {name}");
//                 return null;
//             }
//
//             AudioSource source = audioSourceParam;
//             if (source == null)
//             {
//                 GameObject obj = new GameObject("Sound", typeof(AudioSource));
//                 source = obj.GetComponent<AudioSource>();
//             }
//
//             source.clip = GetRandomAudioClip();
//             source.volume = Random.Range(volume.Min, volume.Max);
//             source.pitch = useSemitones
//                 ? Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, Random.Range(semitones.Min, semitones.Max))
//                 : Random.Range(pitch.Min, pitch.Max);
//
//             source.Play();
// #if UNITY_EDITOR
//             if (source != _previewer) Destroy(source.gameObject, source.clip.length / source.pitch);
// #else
//             Destroy(source.gameObject, source.clip.length / source.pitch);
// #endif
//
//             return source;
//         }
//     }
// }

