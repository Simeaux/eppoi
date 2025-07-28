using System.Collections.Generic;
using UnityEngine;

namespace DigitsNFCToolkit.Samples
{
    public class ReadScreenControl: MonoBehaviour
	{
		public ReadScreenView view;

		public MessageScreenView messageScreenView;

		public void Start()
		{
#if(!UNITY_EDITOR)
			NativeNFCManager.AddNFCTagDetectedListener(OnNFCTagDetected);
			NativeNFCManager.AddNDEFReadFinishedListener(OnNDEFReadFinished);
			NativeNFCManager.AddNDEFMakeReadonlyFinishedListener(OnNDEFMakeReadonlyFinished);
			Debug.Log("NFC Tag Info Read Supported: " + NativeNFCManager.IsNFCTagInfoReadSupported());
			Debug.Log("NDEF Read Supported: " + NativeNFCManager.IsNDEFReadSupported());
			Debug.Log("NDEF Write Supported: " + NativeNFCManager.IsNDEFWriteSupported());
			Debug.Log("NDEF Push Supported: " + NativeNFCManager.IsNDEFPushSupported());
			Debug.Log("NFC Enabled: " + NativeNFCManager.IsNFCEnabled());
			Debug.Log("NDEF Push Enabled: " + NativeNFCManager.IsNDEFPushEnabled());
#endif
		}

		private void OnEnable()
		{
#if(!UNITY_EDITOR) && !UNITY_IOS
			NativeNFCManager.Enable();
#endif
			view.gameObject.SetActive(true);
		}

		private void OnDisable()
		{
#if(!UNITY_EDITOR) && !UNITY_IOS
			NativeNFCManager.Disable();
#endif
			if(view != null)
			{
				view.gameObject.SetActive(false);
			}
		}

		public void OnStartNFCReadClick()
		{
#if(!UNITY_EDITOR)
			NativeNFCManager.ResetOnTimeout = true;
			NativeNFCManager.Enable();
#endif
		}

		public void OnNFCTagDetected(NFCTag tag)
		{
			Debug.Log("1"+ tag.ID);
			view.UpdateTagInfo(tag);
		}

		public void OnNDEFReadFinished(NDEFReadResult result)
		{
            Debug.Log("2" + result.Message);

            if(result.Success)
            {

                List<NDEFRecord> records = result.Message.Records;

                int length = records.Count;
                for (int i = 0; i < length; i++)
                {
                    NDEFRecord record = records[i];
                    string nfc_txt = string.Empty;
                    switch (record.Type)
                    {
                        case NDEFRecordType.TEXT:
                            TextRecord textRecord = (TextRecord)record;
                            nfc_txt = textRecord.text;
                            break;
                        case NDEFRecordType.URI:
                            UriRecord uriRecord = (UriRecord)record;
                            nfc_txt = uriRecord.fullUri;
                            break;
                        case NDEFRecordType.MIME_MEDIA:
                            MimeMediaRecord mimeMediaRecord = (MimeMediaRecord)record;
                            nfc_txt = mimeMediaRecord.mimeType;
                            break;
                        case NDEFRecordType.EXTERNAL_TYPE:
                            nfc_txt = "EXTERNAL_TYPE";
                            //ExternalTypeRecord externalTypeRecord = (ExternalTypeRecord)record;
                            //int dataLength = externalTypeRecord.domainData.Length;
                            //string dataValue = Encoding.UTF8.GetString(externalTypeRecord.domainData);
                            //recordItem.UpdateLabel(string.Format(EXTERNAL_TYPE_FORMAT, NDEFRecordType.EXTERNAL_TYPE, externalTypeRecord.domainName, externalTypeRecord.domainType, dataLength, dataValue));
                            break;
                        case NDEFRecordType.SMART_POSTER:
                            nfc_txt = "SMART_POSTER";
                            //SmartPosterRecord smartPosterRecord = (SmartPosterRecord)record;
                            //int totalRecords = smartPosterRecord.titleRecords.Count + smartPosterRecord.iconRecords.Count + smartPosterRecord.extraRecords.Count;
                            //recordItem.UpdateLabel(string.Format(SMART_POSTER_RECORD_FORMAT, NDEFRecordType.SMART_POSTER, smartPosterRecord.uriRecord.fullUri, smartPosterRecord.action, smartPosterRecord.size, smartPosterRecord.mimeType, totalRecords));
                            break;
                    }
                    PlayerPrefs.SetString("nfc", nfc_txt);
                }

            }
            else
			{
				//readResultString = string.Format("Failed to read NDEF Message from tag {0}\nError: {1}", result.TagID, result.Error);
			}
			Debug.Log("NFC = " + PlayerPrefs.GetString("nfc"));
		}

		public void OnMakeReadonlyClick()
		{
#if(!UNITY_EDITOR)
			NativeNFCManager.RequestNDEFMakeReadonly();
#if UNITY_ANDROID
			messageScreenView.Show();
			messageScreenView.SwitchToPendingMakeReadonly();
#endif
#endif
		}

		public void OnMakeReadonlyOKClick()
		{
			messageScreenView.Hide();
		}

		public void OnMakeReadonlyCancelClick()
		{
			messageScreenView.Hide();
#if(!UNITY_EDITOR) && UNITY_ANDROID
			NativeNFCManager.CancelNDEFMakeReadonlyRequest();
#endif
		}

		public void OnNDEFMakeReadonlyFinished(NDEFMakeReadonlyResult result)
		{
			string makeReadonlyResultString = string.Empty;
			if(result.Success)
			{
				makeReadonlyResultString = string.Format("Tag {0} was successfully made readonly", result.TagID);
			}
			else
			{
				makeReadonlyResultString = string.Format("Failed to make tag {0} readonly\nError: {1}", result.TagID, result.Error);
			}
			Debug.Log(makeReadonlyResultString);
			messageScreenView.SwitchToMakeReadonlyResult(makeReadonlyResultString);
		}
	}
}
