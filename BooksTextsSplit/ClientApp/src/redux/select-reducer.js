const SET_SENTENCES_COUNT = 'SET-SENTENCES-COUNT';
const SET_SENTENCES = 'SET-SENTENCES';
const TOGGLE_IS_FETCHING = 'TOGGLE-IS-FETCHING';

let initialState = {    
    engSentences: [],
    sentencesCount: [777, 888], //engSentencesCount: 777, rusSentencesCount: 888
    emptyVariable: null,
    isTextLoaded: [false, false],
    isFetching: false
}

const selectTextsReducer = (state = initialState, action) => {

    switch (action.type) {        
        case SET_SENTENCES_COUNT:
            {
                let stateCopy = {...state };
                stateCopy.sentencesCount = {...state.sentencesCount };
                stateCopy.sentencesCount[action.languageId] = action.count;
                return stateCopy;
            }
            case SET_SENTENCES:
                {
                    let stateCopy = {...state };
                    stateCopy.engSentences = {...state.engSentences };
                    stateCopy.engSentences = action.sentences;
                    return stateCopy;
                }        
        case TOGGLE_IS_FETCHING:
            {
                return {...state, isFetching: action.isFetching };
            }
        default:
            return state;
    }
}


export const setSentencesCount = (count, languageId) => ({ type: SET_SENTENCES_COUNT, count, languageId });
export const setSentences = (sentences, languageId) => ({ type: SET_SENTENCES, sentences, languageId });

export const toggleIsFetching = (isFetching) => ({ type: TOGGLE_IS_FETCHING, isFetching });

export default selectTextsReducer;