import React from "react";
import { connect } from "react-redux";
import { compose } from "redux";
import { switchBooksIdsOn, switchBookVersionsOn, switchQuickViewOn, nextAfterQuickView } from "../../redux/select-reducer";
import { withAuthRedirect } from "../../hoc/withAuthRedirect";
import SelectTexts from "./SelectTexts";
import Preloader from "../common/preloader/Preloader";
import { withRouter } from "react-router";

class SelectTextsContainer extends React.Component {
  constructor(props) {
    super(props);
  }

  componentDidMount() {
    this.props.switchBooksIdsOn();
  }

  render() {
    return (
      <>
        {this.props.isFetching ? <Preloader /> : null}
        <SelectTexts
          sentencesCount={this.props.sentencesCount}
          allBookNamesSortedByIds={this.props.allBookNamesSortedByIds}
          allVersionsOfBooksNames={this.props.allVersionsOfBooksNames}
          booksPairTexts={this.props.booksPairTexts}
          booksNamesIds={this.props.booksNamesIds}
          isSelectingBookId={this.props.isSelectingBookId}
          isSelectingUploadVersion={this.props.isSelectingUploadVersion}
          isFetching={this.props.isFetching}
          isQuickViewBooksPair={this.props.isQuickViewBooksPair}
          switchBooksIdsOn={this.props.switchBooksIdsOn}
          switchBookVersionsOn={this.props.switchBookVersionsOn}
          switchQuickViewOn={this.props.switchQuickViewOn}
          nextAfterQuickView={this.props.nextAfterQuickView}
        />
      </>
    );
  }
}

let mapStateToProps = (state) => {
  return {
    sentencesCount: state.selectTextsPage.sentencesCount,
    isSelectingBookId: state.selectTextsPage.isSelectingBookId,
    isSelectingUploadVersion: state.selectTextsPage.isSelectingUploadVersion,
    isFetching: state.selectTextsPage.isFetching,
    allBookNamesSortedByIds: state.selectTextsPage.allBookNamesSortedByIds,
    booksNamesIds: state.selectTextsPage.booksNamesIds,
    allVersionsOfBooksNames: state.selectTextsPage.allVersionsOfBooksNames,
    booksPairTexts: state.selectTextsPage.booksPairTexts,
    isQuickViewBooksPair: state.selectTextsPage.isQuickViewBooksPair,
  };
};

export default compose(
  connect(mapStateToProps, { switchBooksIdsOn, switchBookVersionsOn, switchQuickViewOn, nextAfterQuickView }),
  withAuthRedirect,
  /* withRouter */
)(SelectTextsContainer);

