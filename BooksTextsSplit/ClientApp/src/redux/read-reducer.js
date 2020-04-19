const SCROLL_LINE_UP = 'SCROLL-LINE-UP';
const SCROLL_LINE_DOWN = 'SCROLL-LINE-DOWN';
const SET_SENTENCES_COUNT = 'SET-SENTENCES-COUNT';
const SET_SENTENCES = 'SET-SENTENCES';
const TOGGLE_IS_FETCHING = 'TOGGLE-IS-FETCHING';
const TOGGLE_IS_LOADING = 'TOGGLE-IS-LOADING';

let initialState = {
    engSentences: [],
    lastSentenceNumber: 32,
    rusSentences: [],
    sentencesOnPage: 20,
    sentencesOnPageTop: 10,
    sentencesOnPageBottom: 10,
    readingSentenceNumber: null,
    isFetching: false,
    sentencesCount: [0, 0],
    emptyVariable: null
}

initialState.readingSentenceNumber = initialState.sentencesOnPageTop;
let lastSentenceNumber = initialState.lastSentenceNumber;

const readAndTranslateReducer = (state = initialState, action) => {    
    let sentencesOnPageTop = initialState.sentencesOnPageTop;    
    switch (action.type) {
        case SCROLL_LINE_UP: {
            let newSentenceNumberMinus = action.newSentenceNumber > sentencesOnPageTop
                ? action.newSentenceNumber - 1
                : action.newSentenceNumber;
            let stateCopy = { ...state, readingSentenceNumber: newSentenceNumberMinus };
            return stateCopy;
        }
        case SCROLL_LINE_DOWN: {
            let newSentenceNumberPlus = action.newSentenceNumber < (lastSentenceNumber + sentencesOnPageTop - 1)
                ? action.newSentenceNumber + 1
                : action.newSentenceNumber;
            let stateCopy = { ...state, readingSentenceNumber: newSentenceNumberPlus };
            return stateCopy;
        }
        case TOGGLE_IS_LOADING: {
            /* return { ...state, isEngLoaded: action.isEngLoaded } */
            let stateCopy = { ...state };
            stateCopy.isTextLoaded = { ...state.isTextLoaded };
            stateCopy.isTextLoaded[action.languageId] = action.isTextLoaded;
            return stateCopy;
        }
        case SET_SENTENCES_COUNT: {
            let stateCopy = { ...state };
            stateCopy.sentencesCount = { ...state.sentencesCount };
            stateCopy.sentencesCount[action.languageId] = action.count;
            //lastSentenceNumber = stateCopy.sentencesCount[0];
            return stateCopy;
        }
        case SET_SENTENCES: {
            let stateCopy = { ...state };
            switch (action.languageId) {
                case 0:
                    return {
                        ...state,
                        engSentences: action.sentences
                    }
                case 1:
                    return {
                        ...state,
                        rusSentences: action.sentences
                    }
                default:
                    return stateCopy;
            }
        }
        case TOGGLE_IS_FETCHING: {
            return { ...state, isFetching: action.isFetching };
        }
        default:
            return state;
    }
}

export const scrollLineUp = (newSentenceNumber) => ({ type: SCROLL_LINE_UP, newSentenceNumber });
export const scrollLineDown = (newSentenceNumber) => ({ type: SCROLL_LINE_DOWN, newSentenceNumber });
export const toggleIsLoading = (isTextLoaded, languageId) => ({ type: TOGGLE_IS_LOADING, isTextLoaded, languageId });
export const setSentencesCount = (count, languageId) => ({ type: SET_SENTENCES_COUNT, count, languageId });
export const setSentences = (sentences, languageId) => ({ type: SET_SENTENCES, sentences, languageId });
export const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });

export default readAndTranslateReducer;
