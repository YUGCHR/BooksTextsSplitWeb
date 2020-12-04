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
          allBookVersions={this.props.allBookVersions}
          booksPairTexts={this.props.booksPairTexts}
          booksNamesIds={this.props.booksNamesIds}
          isSelectBookId={this.props.isSelectBookId}
          isSelectVersion={this.props.isSelectVersion}
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
    isSelectBookId: state.selectTextsPage.isSelectBookId,
    isSelectVersion: state.selectTextsPage.isSelectVersion,
    isFetching: state.selectTextsPage.isFetching,
    allBookNamesSortedByIds: state.selectTextsPage.allBookNamesSortedByIds,
    booksNamesIds: state.selectTextsPage.booksNamesIds,
    allBookVersions: state.selectTextsPage.allBookVersions,
    booksPairTexts: state.selectTextsPage.booksPairTexts,
    isQuickViewBooksPair: state.selectTextsPage.isQuickViewBooksPair,
  };
};

export default compose(
  connect(mapStateToProps, { switchBooksIdsOn, switchBookVersionsOn, switchQuickViewOn, nextAfterQuickView }),
  withAuthRedirect,
  /* withRouter */
)(SelectTextsContainer);

