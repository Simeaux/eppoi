#import <AVFoundation/AVFoundation.h>

// Istanza statica per mantenere il riferimento tra le chiamate
static AVSpeechSynthesizer *synth = nil;

extern "C" {
    // Funzione per far partire l'audio
    void _iosSpeak(const char* text, const char* language) {
        if (synth == nil) {
            synth = [[AVSpeechSynthesizer alloc] init];
        }
        
        // Se sta già parlando, interrompiamo per far partire il nuovo (opzionale)
        if ([synth isSpeaking]) {
            [synth stopSpeakingAtBoundary:AVSpeechBoundaryImmediate];
        }

        NSString *message = [NSString stringWithUTF8String:text];
        AVSpeechUtterance *utterance = [AVSpeechUtterance speechUtteranceWithString:message];
        
        // Configurazione voce
        utterance.voice = [AVSpeechSynthesisVoice voiceWithLanguage:[NSString stringWithUTF8String:language]];
        utterance.rate = AVSpeechUtteranceDefaultSpeechRate; 
        
        [synth speakUtterance:utterance];
    }
     // Funzione dedicata per fermare l'audio
    void _iosStopSpeak() {
        if (synth != nil && [synth isSpeaking]) {
            // AVSpeechBoundaryImmediate interrompe all'istante
            // AVSpeechBoundaryWord interrompe alla fine della parola corrente
            [synth stopSpeakingAtBoundary:AVSpeechBoundaryImmediate];
        }
    }
}